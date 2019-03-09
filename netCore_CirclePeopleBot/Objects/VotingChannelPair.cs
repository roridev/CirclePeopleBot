using System;
using System.Collections.Generic;
using System.Text;

namespace netCore_CirclePeopleBot.Objects
{
    public class VotingChannelPair
    {
        public ulong InputChannelID { get; set; }
        public ulong ApprovedChannelID { get; set; }
        public String PairName { get; set; }
    }
}
