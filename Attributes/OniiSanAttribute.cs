using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Lolibase.Discord.Attributes
{
    public class OniiSanAttribute : CheckBaseAttribute
    {
        public async override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return ctx.Guild.Id.Equals(Program.Config.MasterId);
        }
    }
}