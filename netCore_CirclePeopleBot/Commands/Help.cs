using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace netCore_CirclePeopleBot.Commands
{
    [Group("help")]
    public class Help : BaseCommandModule
    {
        [GroupCommand]
        public async Task HelpCommand(CommandContext ctx)
        {
            bool isStaff = ctx.Member.IsStaff(ctx.Channel);
            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
            Embed
                .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                .WithDescription($"Description of a command : **{ctx.Prefix}**help `<Command>`")
                .WithAuthor($"Listing all Commands / Modules", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
            if (isStaff)
            {
                Embed.AddField($"Administrative Commands", $"{ctx.Prefix}config");
            }

            await ctx.RespondAsync(embed: Embed);
        }
    }
}
