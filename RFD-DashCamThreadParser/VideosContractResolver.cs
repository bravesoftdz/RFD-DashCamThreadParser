using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFD_DashCamThreadParser
{
    // http://stackoverflow.com/a/25158578/342378
    public class VideosContractResolver : DefaultContractResolver
    {
        private static string[] _MemberPropertiesToIgnore = { "FirstPost", "LastPost", "TextPosts", "VideoPosts" };

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            if (type == typeof(Member))
            {
                properties = properties.Where(p => !_MemberPropertiesToIgnore.Contains(p.PropertyName)).ToList();
            }
            return properties;
        }
    }
}
