using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace CirclePeopleBot.Objects
{
    public static class EmojiExtension
    {
        public static async Task<DiscordEmoji> FetchEmoji(this DiscordGuild g, ulong u)
        {
            DiscordEmoji e = null;
            try
            {
                e = await g.GetEmojiAsync(u);
                return e;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}