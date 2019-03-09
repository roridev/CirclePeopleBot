using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace netCore_CirclePeopleBot
{
    public class MainCFG
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("enable_dms")]
        public bool EnableDMs { get; set; }
        [JsonProperty("prefixes")]
        public IEnumerable<String> Prefixes { get; set; }
    }
}
