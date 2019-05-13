using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CirclePeopleBot.Objects
{
    public class StaffAttribute : CheckBaseAttribute
    {
#pragma warning disable 1998
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
#pragma warning restore 1998
        {
            return ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles);
        }
    }
}