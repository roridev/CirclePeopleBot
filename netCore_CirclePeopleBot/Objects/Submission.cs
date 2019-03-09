using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace netCore_CirclePeopleBot.Objects
{
    public class Submission
    {
        public string SuggestionText { get; set; }
        public string ImageURL { get; set; }
        public DiscordMember Author { get; set; }
    }
}
