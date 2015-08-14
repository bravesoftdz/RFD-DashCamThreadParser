using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RFD_DashCamThreadParser
{
    class Program
    {
        private static Regex PostRegex = new Regex("[<]li class[=]\"postbitlegacy postbitim postcontainer\" id[=]\"post_(\\d+)\"[>](.*?)[<][!][-][-] ADSENSE AFTER FIRST POST [-][-][>]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex AuthorRegex = new Regex("[<]a rel[=]\"nofollow\" class[=]\".*?\" href[=]\"(.*?)\" title=\".*?\"[>][<]strong[>](.*?)[<][/]strong[>][<][/]a[>]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex DateRegex = new Regex("[<]span class=\"date\"[>](.*?)[&]nbsp[;][<]span class[=]\"time\"[>](.*?)[<][/]span[>][<][/]span[>]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex PostedByRegex = new Regex("[<]div class[=]\"bbcode_postedby\"[>](.*?)[<][/]div[>]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex ParentIdRegex = new Regex("[/][#]post(\\d+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex QuoteRegex = new Regex("[<]div class[=]\"message\"[>].*?[<][/]div[>]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex iframeRegex = new Regex("[<]iframe .*? src[=]\"//www.youtube.com/embed/(.*?)[?]wmode[=]opaque\".*?[>][<][/]iframe[>]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static List<Post> _Posts = new List<Post>();

        static void Main(string[] args)
        {
            string HtmlFilesPath = @"..\..\HtmlFiles";
            int PageCount = Directory.EnumerateFiles(HtmlFilesPath, "Page*.html").Count();

            for (int PageNumber = 1; PageNumber <= PageCount; PageNumber++)
            {
                string Filename = String.Format(@"{0}\Page{1}.html", HtmlFilesPath, PageNumber);
                Console.WriteLine("- Parsing {0}", Filename);

                string Html = File.ReadAllText(Filename);
                var PostMatches = PostRegex.Matches(Html);
                if (PostMatches.Count == 15)
                {
                    foreach (Match PostMatch in PostMatches)
                    {
                        if (PostMatch.Groups[0].Success)
                        {
                            // Remove the signature (if there is one)
                            string PostHtml = PostMatch.Groups[2].Value.Split(new string[] { "<div class=\"after_content\">" }, StringSplitOptions.None)[0];
                            int PostId = int.Parse(PostMatch.Groups[1].Value);

                            _Posts.Add(new Post()
                            {
                                Id = PostId,
                                Author = GetAuthor(PostHtml, PostId),
                                Date = GetDate(PostHtml, PostId),
                                Page = PageNumber,
                                ParentIds = GetParentIds(PostHtml, PostId),
                                VideoUrls = GetVideoUrls(PostHtml, PostId)
                            });
                        }
                        else
                        {
                            Console.WriteLine("  * Didn't find post id and body!");
                            Console.ReadKey();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("  * Only found {0} posts!", PostMatches.Count);
                    Console.ReadKey();
                }
            }

            // Now get the number of replies
            Console.WriteLine("- Updating ReplyCounts...");
            for (int i = _Posts.Count - 1; i >= 0; i--)
            {
                Console.Write(" {0}", i);

                Post Post = _Posts[i];
                Post.ReplyCountDirect = _Posts.Where(x => x.ParentIds.Contains(Post.Id)).Count();
                Post.ReplyCountTotal = Post.ReplyCountDirect + _Posts.Where(x => x.ParentIds.Contains(Post.Id)).Select(x => x.ReplyCountTotal).Sum();
            }

            Console.WriteLine();
            Console.WriteLine("- Saving file...");
            File.WriteAllText("RFD-DashCamVideos.json", "{\"data\":" + JsonConvert.SerializeObject(_Posts.Where(x => x.VideoUrls.Count > 0)) + "}");

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Hit a key to quit...");
                Console.ReadKey();
            }
        }

        private static Author GetAuthor(string html, int postId)
        {
            Match AuthorMatch = AuthorRegex.Match(html);
            if (AuthorMatch.Success && AuthorMatch.Groups[0].Success)
            {
                return new Author()
                {
                    Name = AuthorMatch.Groups[2].Value,
                    Url = AuthorMatch.Groups[1].Value
                };
            }
            else
            {
                Console.WriteLine("  * Didn't find post author for {0}", postId);
                Console.ReadKey();
            }

            return null;
        }

        private static DateTime GetDate(string html, int postId)
        {
            Match DateMatch = DateRegex.Match(html);
            if (DateMatch.Success && DateMatch.Groups[0].Success)
            {
                return DateTime.Parse(DateMatch.Groups[1].Value.Replace("st,", ",").Replace("nd,", ",").Replace("rd,", ",").Replace("th,", ",") + " " + DateMatch.Groups[2].Value);
            }
            else
            {
                Console.WriteLine("  * Didn't find post date for {0}", postId);
                Console.ReadKey();
            }

            return DateTime.MinValue;
        }

        private static List<int> GetParentIds(string postHtml, int postId)
        {
            var Result = new List<int>();

            var PostedByMatches = PostedByRegex.Matches(postHtml);
            foreach (Match PostedByMatch in PostedByMatches)
            {
                if (PostedByMatch.Groups[0].Success)
                {
                    Match ParentIdMatch = ParentIdRegex.Match(PostedByMatch.Groups[1].Value);
                    if (ParentIdMatch.Success && ParentIdMatch.Groups[0].Success)
                    {
                        Result.Add(int.Parse(ParentIdMatch.Groups[1].Value));
                    }
                    else
                    {
                        Console.WriteLine("  * Error handling 'Originally posted by...' for {0}", postId);
                        // These mean someone quoted manually so there's no link back to the parent...don't bother prompting 
                        // Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("  * Error handling 'Originally posted by...' for {0}", postId);
                    Console.ReadKey();
                }
            }

            return Result;
        }

        private static List<string> GetVideoUrls(string postHtml, int postId)
        {
            var Result = new List<string>();

            // Remove Quotes, so we don't attribute a video to someone replying with a video in a quote
            postHtml = QuoteRegex.Replace(postHtml, "");

            var iframeMatches = iframeRegex.Matches(postHtml);
            foreach (Match iframeMatch in iframeMatches)
            {
                if (iframeMatch.Groups[0].Success)
                {
                    Result.Add(iframeMatch.Groups[1].Value.ToString());
                }
                else
                {
                    Console.WriteLine("  * Error handling YouTube iframe for {0}", postId);
                    Console.ReadKey();
                }
            }

            return Result;
        }
    }
}
