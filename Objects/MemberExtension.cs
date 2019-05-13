using DSharpPlus;
using DSharpPlus.Entities;

namespace CirclePeopleBot.Objects
{
    public static class MemberExtension
    {
        public static bool IsStaff(this DiscordMember m, DiscordChannel c)
        {
            return m.PermissionsIn(c).HasPermission(Permissions.ManageRoles);
        }
    }
}