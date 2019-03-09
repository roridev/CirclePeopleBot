using DSharpPlus;
using DSharpPlus.Entities;
using netCore_CirclePeopleBot.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace netCore_CirclePeopleBot
{
    public static class CustomExtensions
    {
        public static bool IsStaff(this DiscordMember Member, DiscordChannel channel)
        {
            if (!Member.IsBot)
            {
                return Member.PermissionsIn(channel).HasPermission(Permissions.ManageRoles);
            }
            else {
                return false;
            }
           
        }


        public static  async Task<Submission> GetSubmissionByMessage(this DiscordMessage Message)
        {
            Submission innerSub = new Submission();
            innerSub.Author = await Message.Channel.Guild?.GetMemberAsync(Message.Author.Id);
            if (Message.Attachments.Count == 0 && Message.Content.Contains("http"))
            {

                int pos = -1;
                pos = Message.Content.IndexOf("http");
                string part1 = Message.Content.Substring(0, pos);
                string part2 = Message.Content.Substring(pos, Message.Content.Length - pos);

                if (part1.StartsWith("http"))
                {
                    innerSub.ImageURL = part1;
                    innerSub.SuggestionText = (String.IsNullOrWhiteSpace(part2) ? "No Text ;(" : part2);

                }
                else
                {
                    innerSub.ImageURL = part2;
                    innerSub.SuggestionText = innerSub.SuggestionText = (String.IsNullOrWhiteSpace(part1) ? "No Text ;(" : part1);
                }

                return innerSub;
            }
            else if (Message.Attachments.Count == 1)
            {
                innerSub.ImageURL = Message.Attachments[0].Url;
                innerSub.SuggestionText = (String.IsNullOrWhiteSpace(Message.Content) ? "No Text ;(" : Message.Content);
                return innerSub;
            }
            else {
                innerSub.ImageURL = "Invalid Submission";
                innerSub.SuggestionText = "Invalid Submission";
                return innerSub;
            }
        }
    }

}
