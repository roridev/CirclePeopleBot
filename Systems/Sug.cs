using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Lolibase.Discord.Utils;
using Lolibase.Objects;

namespace Lolibase.Discord.Systems
{
    public class Sug : IApplyToClient, IApplicableSystem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        public void Activate()
        {
            Active = true;
            Name = "Suggestion";
            Description = "CirclePeople's Infopeople System";
        }

        public void ApplyToClient(DiscordClient client)
        {
            if (Active)
            {
                client.MessageCreated += MessageCreated;
                client.MessageReactionAdded += ReactionAdded;
                client.GuildDownloadCompleted += Ready;
            }

        }

        private async Task Ready(GuildDownloadCompletedEventArgs e)
        {
            var pairs = Program.pairs;
            var client = e.Client;
            var Config = Program.Config;
            if (pairs.Count > 0)
            {
                Console.WriteLine("[Reading Previous Suggestions] Started");
                var guild = await client.GetGuildAsync(Config.MasterId);
                foreach (var pair in pairs)
                {
                    Console.WriteLine($"[Reading Previous Suggestions] {pair.Name}");
                    var ich = await client.GetChannelAsync(pair.InputPair);
                    var och = await client.GetChannelAsync(pair.OutputPair);
                    var hundred = await ich.GetMessagesAsync();
                    var list = hundred.ToList();
                    foreach (var msg in list)
                    {
                        if (!String.IsNullOrWhiteSpace(msg.Content))
                        {
                            Suggestion s = new Suggestion(msg);
                            if (s.Image.HasValue && s.Links.Count > 0 && msg.Reactions.Count > 0)
                            {
                                var ppbusg3h = DiscordEmoji.FromName(e.Client, ":+1:");
                                if (msg.Reactions.Any(z => z.Emoji == ppbusg3h))
                                {
                                    List<DiscordUser> reacts = new List<DiscordUser>();
                                    reacts = (await (await ich.GetMessageAsync(msg.Id)).GetReactionsAsync(ppbusg3h)).ToList();
                                    
                                    if (reacts.Any( x  => ((guild.GetMemberAsync(x.Id).Result).PermissionsIn(ich).HasPermission(Permissions.ManageRoles))))
                                    {
                                        MemoryStream str = new MemoryStream(s.Image.Value);
                                        await och.SendFileAsync("temp.png", str, embed: EmbedBase.SuggestionEmbed(s));
                                        str.Dispose();
                                        await msg.DeleteAsync();
                                    }
                                }
                            }
                        }

                    }
                    Console.WriteLine($"[Reading Previous Suggestions] {pair.Name} <COMPLETED>");
                }
            }
        }
        private async Task MessageCreated(MessageCreateEventArgs e)
        {
            if (Program.pairs.Count != 0 && Program.pairs.Any(x => x.InputPair == e.Channel.Id))
            {
                Suggestion sg = new Suggestion(e.Message);
                if (sg.Image.HasValue || sg.Links.Count != 0)
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":+1:"));
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":-1:"));
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":white_check_mark:"));
                }
            }
        }

        private async Task ReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User != e.Client.CurrentUser && Program.pairs.Any(x => x.InputPair == e.Channel.Id))
            {
                Pair p = Program.pairs.Find(x => x.InputPair == e.Channel.Id);
                if (e.Emoji == DiscordEmoji.FromName(e.Client, ":+1:") && ((DiscordMember)e.User).PermissionsIn(e.Channel).HasPermission(Permissions.ManageRoles))
                {
                    var msg = await e.Channel.GetMessageAsync(e.Message.Id);
                    Suggestion sg = new Suggestion(msg);
                    Stream s = new MemoryStream(sg.Image.Value);
                    var RetChn = e.Channel.Guild.GetChannel(p.OutputPair);
                    if (sg.Image.HasValue)
                    {
                        await RetChn.SendFileAsync("temp.png", s, null, false, EmbedBase.SuggestionEmbed(sg));
                        await e.Message.DeleteAsync();
                    }
                    s.Dispose();
                }
            }
        }
        public void Deactivate()
        {
            Active = false;
        }
    }
}