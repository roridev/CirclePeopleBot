using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirclePeopleBot.Models
{
    public class YoutubeConfig
    {
        public ulong BroadcastID { get; set; }
        public string ChannelURL { get; set; }
        public string UploadListURL { get; set; }
        public string LastVideoID { get; set; }
    }
}
