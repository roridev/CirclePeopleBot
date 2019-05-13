using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CirclePeopleBot.Objects;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Newtonsoft.Json;

namespace CirclePeopleBot
{   
    class Program
    {
        public static DiscordClient Client{ get; set; }
        public static InteractivityExtension Interactivity {get;set;}
        public static CommandsNextExtension CommandsNext { get; set; }
        public static Config MainCfg { get; private set; }
        
        public static Program Instance { get; set; }
        public static List<Pair> Pairs { get; set; }
        public static void Main(string[] args)
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + @"/Config.json"))
            {
                MainCfg = ConfigScreen();
                File.WriteAllText($@"{Directory.GetCurrentDirectory()}/Config.json",JsonConvert.SerializeObject(MainCfg, Formatting.Indented));
            }
            else
            {
                MainCfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText($@"{Directory.GetCurrentDirectory()}/Config.json"));
            }

            if (!File.Exists(Directory.GetCurrentDirectory() + @"/Pairs.json"))
            {
                Pairs = new List<Pair>();
                File.WriteAllText($@"{Directory.GetCurrentDirectory()}/Pairs.json",JsonConvert.SerializeObject(Pairs, Formatting.Indented));
            }
            else
            {
                Pairs = JsonConvert.DeserializeObject<List<Pair>>(File.ReadAllText($@"{Directory.GetCurrentDirectory()}/Pairs.json"));
            }

            Instance = new Program();
            Instance.StartAsync().GetAwaiter().GetResult();
            
        }

        private Program()
        {
            Client = new DiscordClient(new DiscordConfiguration{
                AutoReconnect =  true,
                UseInternalLogHandler = true,
                Token = MainCfg.Token,
                TokenType = TokenType.Bot
            });

            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(10),
                PaginationBehaviour =  PaginationBehaviour.Ignore,
                PaginationDeletion = PaginationDeletion.DeleteEmojis
            });

            CommandsNext = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefixAsync,
                EnableDms =  MainCfg.AllowDms
            });
            
            CommandsNext.RegisterCommands(Assembly.GetEntryAssembly());
            //DiscordAPI EventHandlers

            Client.MessageCreated += MessageCreated;
            Client.MessageReactionAdded += ReactionAdded;
            Client.GuildDownloadCompleted += GuildDownloadCompleted;
        }

        private async Task GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
        {
            if (Pairs.Count > 0)
            {
                foreach (var pair in Pairs)
                {
                    var status = await pair.Initialize(Client);
                    if (status)
                    {
                        Console.WriteLine($"[INFO] Initialization of {pair.PairName} completed without errors.");
                    }
                    else
                    {
                        Console.WriteLine($"[INFO] Initialization of {pair.PairName} failed.");
                    }
                } 
            }
            
        }

        private async Task ReactionAdded(MessageReactionAddEventArgs e)
        {
            if (Pairs.Any(x => x.FromChannel == e.Channel.Id))
            {
                DiscordEmoji upvoteEmoji = await e.Channel.Guild.FetchEmoji(MainCfg.UpvoteId) ?? DiscordEmoji.FromUnicode(Client,"👍");
                DiscordEmoji downvoteEmoji = await e.Channel.Guild.FetchEmoji(MainCfg.DownvoteId) ?? DiscordEmoji.FromUnicode(Client, "👎");
                var toChannel = e.Channel.Guild.GetChannel(Pairs.Find(x => x.FromChannel == e.Channel.Id).ToChannel);
                var member = await e.Channel.Guild.GetMemberAsync(e.User.Id);
                if (e.Emoji == upvoteEmoji && member.IsStaff(e.Channel))
                {
                    var (submission, status) = e.Message.GetSubmission();
                    if (status)
                    {
                        var op = await toChannel.Guild.GetMemberAsync(submission.OpId);
                        var embedBuilder = new DiscordEmbedBuilder();
                        var opName = op.Nickname ?? op.Username;
                        embedBuilder
                            .WithDescription($"About :\n{submission.Description}")
                            .WithColor(new DiscordColor(150, 202, 255))
                            .WithFooter($"Submitted by : {opName}#{op.Discriminator}")
                            .WithAuthor("Score approved",submission.ImageLink);
                        if (submission.ImageLink.Contains(".png")||submission.ImageLink.Contains(".jpg") || submission.ImageLink.Contains("puu.sh"))
                        {
                            embedBuilder.WithImageUrl(submission.ImageLink);
                        }
                        else
                        {
                            embedBuilder.AddField("Link", submission.ImageLink);
                        }

                        await toChannel.SendMessageAsync(embed: embedBuilder.Build());
                        await e.Message.DeleteAsync();
                    }
                    else
                    {
                        await e.Message.DeleteReactionAsync(e.Emoji,e.User);
                    }

                }

                if (e.Emoji == downvoteEmoji && member.IsStaff(e.Channel))
                {
                    await e.Message.DeleteAsync();
                }
            }
        }

        private async Task MessageCreated(MessageCreateEventArgs e)
        {
            if (Pairs.Any(x => x.FromChannel == e.Channel.Id))
            {
                DiscordEmoji upvoteEmoji = await e.Channel.Guild.FetchEmoji(MainCfg.UpvoteId) ?? DiscordEmoji.FromUnicode(Client,"👍");
                DiscordEmoji downvoteEmoji = await e.Channel.Guild.FetchEmoji(MainCfg.DownvoteId) ?? DiscordEmoji.FromUnicode(Client, "👎");
                var (submission,status)=e.Message.GetSubmission();
                if (status)
                {
                    await e.Message.CreateReactionAsync(upvoteEmoji);
                    await e.Message.CreateReactionAsync(downvoteEmoji);
                }
                else
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(Client,"⚠"));
                }
            }
        }

#pragma warning disable 1998
        private static async Task<int> ResolvePrefixAsync(DiscordMessage msg)
#pragma warning restore 1998
        {
            var response = -1;
            response = msg.GetMentionPrefixLength(Client.CurrentUser);
            if (response != -1)
            {
                return response;
            }
            else
            {
                foreach (var prefix in MainCfg.Prefixes)
                {
                    response = msg.GetStringPrefixLength(prefix);
                    if (response == -1) continue;
                    return response;
                }
            }

            return response;
        }

        private static Config ConfigScreen()
        {
            Config cfg = new Config();
            Console.WriteLine("Welcome to the Configuration Screen");
            Console.WriteLine("Please type the token:\n\n");
            cfg.Token = Console.ReadLine();
            Console.WriteLine("Please type the prefixes(comma-separated):\n\n");
            cfg.Prefixes = Console.ReadLine()?.Split(',').ToList();
            Console.Clear();
            Console.WriteLine("Configuration Completed.");
            cfg.AllowDms = true;
            cfg.DownvoteId = 0;
            cfg.UpvoteId = 0;
            return cfg;
        }

        private  async Task StartAsync()
        {
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
