using DSharpPlus;

namespace Lolibase.Discord.Utils
{
    /// <summary>
    /// Interface for <see cref="IApplicableSystem"/> that can be applied to <see cref="DiscordClient"/>
    /// </summary>
    public interface IApplyToClient
    {
        string Name {get;set;}
        string Description {get;set;}
        bool Active {get;set;}
        void ApplyToClient(DiscordClient client);

        void Activate();
        void Deactivate();
    }
}