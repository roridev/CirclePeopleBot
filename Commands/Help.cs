using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Lolibase.Discord.Utils;

namespace Lolibase.Discord.Commands
{
    public class Help : BaseCommandModule
    {
        // This is a sample help command.
        [Command("help")]
        public async Task helpCommand(CommandContext ctx)
        {
            await ctx.RespondAsync(embed:ctx.CommandsNext.HelpEmbed());
        }
    }
}