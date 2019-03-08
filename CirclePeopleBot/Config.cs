using DSharpPlus;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CirclePeopleBot
{
    public class Config
    {
        [JsonProperty("token")]
        public string Token
        {
            get; set;
        }

        [JsonProperty("token_type")]
        public TokenType TokenType
        {
            get; set;
        }

        [JsonProperty("prefix")]
        public IEnumerable<string> Prefix
        {
            get; set;
        }

        [JsonProperty("enable_dms")]
        public bool EnableDms
        {
            get; set;
        }
    }
}