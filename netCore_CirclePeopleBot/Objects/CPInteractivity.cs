using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace netCore_CirclePeopleBot.Objects
{
    public class CPInteractivity
    {
        public async Task<DiscordMessage> GetResponce(InteractivityExtension interact, CommandContext ctx)
        {
            var responce = await interact.WaitForMessageAsync(m => m.Author == ctx.Member, TimeSpan.FromMinutes(10));
            return responce.Message;
        }

        public async Task<DiscordMessage> GetPrivateResponce(InteractivityExtension interact, CommandContext ctx)
        {
            DiscordChannel dm = await ctx.Member.CreateDmChannelAsync();
            var responce = await interact.WaitForMessageAsync(m => m.Author == ctx.Member && m.Channel == dm, TimeSpan.FromMinutes(15));
            return responce.Message;
        }
    }
}
