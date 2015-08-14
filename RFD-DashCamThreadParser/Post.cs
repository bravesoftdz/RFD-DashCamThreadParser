using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFD_DashCamThreadParser
{
    class Post
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Member Member { get; set; }
        public int Page { get; set; }
        [JsonIgnore]
        public List<int> ParentIds { get; set; }
        public int ReplyCountDirect { get; set; }
        public int ReplyCountTotal { get; set; }
        [JsonIgnore]
        public List<string> VideoUrls { get; set; }
    }
}
