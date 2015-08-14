using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFD_DashCamThreadParser
{
    class Member
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime FirstPost { get; set; }
        public DateTime LastPost { get; set; }
        public int TextPosts { get; set; }
        public int VideoPosts { get; set; }
    }
}
