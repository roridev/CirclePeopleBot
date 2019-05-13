using System.Collections.Generic;
using System.Security.Cryptography;

namespace CirclePeopleBot
{
    using Newtonsoft.Json;
    
    public class Config
    {
        [JsonProperty] public string Token { get; set; }
        [JsonProperty] public bool AllowDms { get; set; }
        [JsonProperty] public List<string> Prefixes { get; set; }
        [JsonProperty] public ulong UpvoteId { get; set; }
        [JsonProperty] public ulong DownvoteId { get; set; }
    }
}