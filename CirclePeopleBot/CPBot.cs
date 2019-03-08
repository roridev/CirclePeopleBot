using CirclePeopleBot.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CirclePeopleBot
{
    public class CPBot
    {
        public static CPBot Instance { get; set; }
        public static CommandsNextExtension CommandsNext { get; set; }
        public static InteractivityExtension Interactivity { get; set; }
        public static Config MainCFG { get; set; }
        public static DiscordClient Client { get; set; }
        public static List<DiscordSystem> Systems { get; set; }

        static void Main(string[] args)
        {
            if (File.Exists($@"{Directory.GetCurrentDirectory()}\SystemsConfig.json"))
            {
                Systems = JsonConvert.DeserializeObject<List<DiscordSystem>>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\SystemsConfig.json"));
                
            }
            else
            {
                Systems = new List < DiscordSystem > { new DiscordSystem { inputID = 0, aprovedID = 0 , name = "INITIAL PLACEHOLDER, DONT WORRY IT WILL DELETE ITSELF WHEN YOU CREATE YOUR FIRST SYSTEM"} };
                
            }
            if (!File.Exists($@"{Directory.GetCurrentDirectory()}\Config.json")) {
                //Configuration
                Console.WriteLine($"Welcome to the Configuration Screen for the bot.\n" +
                    $"Please Type the token that will be used for the connection and press enter :\n");
                String CfgToken = Console.ReadLine();
                Console.WriteLine($"\n\nToken set!\nType the prefix/prefixes (comma [,] separated) that will be used by the bot :\n");
                String buffer = Console.ReadLine();
                List<String> Prefixes =(buffer.Contains(",")?buffer.Split(',').ToList():new List<string> { buffer });
                if (Prefixes.Count == 1)
                {
                    Console.WriteLine($"\n\nPrefix {Prefixes[0]} set! Configuration Done.\n");
                    
                }
                else {
                    String streamlined = "";
                    Prefixes.ForEach(x => streamlined += $"{x} , ");
                    Console.WriteLine($"\n\nPrefixes {streamlined}. set! Configuration Done.\n");
                    
                }

                Config newCfg = new Config {EnableDms = true, Prefix = Prefixes, Token = CfgToken,TokenType = TokenType.Bot };
                File.WriteAllText($@"{Directory.GetCurrentDirectory()}\Config.json", JsonConvert.SerializeObject(newCfg));
                MainCFG = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Config.json"));


                    Instance = new CPBot();
                    Instance.StartAsync().GetAwaiter().GetResult();




            }
            else {
                MainCFG = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Directory.GetCurrentDirectory()+@"\Config.json"));

                    Instance = new CPBot();
                    Instance.StartAsync().GetAwaiter().GetResult();
                
            }
            

        }
        public CPBot() {
            Client = new DiscordClient(new DiscordConfiguration
            {
                UseInternalLogHandler = true,
                Token = MainCFG.Token,
                TokenType = MainCFG.TokenType,
            });

            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions,
                PaginationTimeout = TimeSpan.FromMinutes(10),
                Timeout = TimeSpan.FromMinutes(30)
            });

            CommandsNext = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefixAsync,
                StringPrefixes = MainCFG.Prefix,
                EnableDms = MainCFG.EnableDms,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            });
        async Task<int> ResolvePrefixAsync(DiscordMessage msg)
        {
            return msg.GetStringPrefixLength(MainCFG.Prefix.ToList()[0]);
        }

        CommandsNext.RegisterCommands(Assembly.GetEntryAssembly());
            /*
             * Discord Events
             */

            Client.MessageCreated += MessageCreated;
            Client.MessageReactionAdded += ReactionAdded;

        }


        private async Task ReactionAdded(DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            if (Systems[0].inputID != 0 && Systems.Find(x => x.inputID == e.Channel.Id) != null)
            {
                DiscordSystem System = Systems.Find(x => x.inputID == e.Channel.Id);
                DiscordMember member = await e.Channel.Guild.GetMemberAsync(e.User.Id);
                if (member.PermissionsIn(e.Channel).HasPermission(Permissions.ManageRoles) &&!member.IsBot)
                {
                    if (e.Emoji.Name == "👍")
                    {
                        String suggestion = e.Message.Content;
                        String author = e.Message.Author.Username + "#" + e.Message.Author.Discriminator;
                        DiscordEmbedBuilder transferedMessage = new DiscordEmbedBuilder();
                        DiscordChannel approvedChannel = e.Channel.Guild.GetChannel(System.aprovedID);
                        DiscordAttachment attachment = null;
                        if (e.Message.Attachments.Count != 0)
                        {
                            attachment = e.Message.Attachments[0];
                        }
                       
                        
                        transferedMessage
                            .WithFooter($"Post by : {author}")
                            .WithDescription(String.IsNullOrWhiteSpace(suggestion)?"No Message :(": suggestion)
                            .WithImageUrl(attachment == null ? "https://i.imgur.com/a76ETJA.png" : attachment.Url)
                            .WithAuthor($"Post Approved");
                        DiscordMessage sentMessage = await approvedChannel.SendMessageAsync("@here",embed: transferedMessage);
                        await sentMessage.CreateReactionAsync(DiscordEmoji.FromUnicode(CPBot.Client, "👍"));
                        await sentMessage.CreateReactionAsync(DiscordEmoji.FromUnicode(CPBot.Client, "👎"));
                        await e.Message.DeleteAsync();

                    }
                    else if (e.Emoji.Name == "👎")
                    {
                        await e.Message.DeleteAsync();
                    }
                }

            }

        }

        private async Task MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (Systems[0].inputID != 0 && Systems.Find(x => x.inputID == e.Channel.Id) != null)
            {
                DiscordMember member = await e.Channel.Guild.GetMemberAsync(e.Message.Author.Id);
                if (!member.IsBot)
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(CPBot.Client, "👍"));
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(CPBot.Client, "👎"));
                }
            }
        }

        public async Task StartAsync ()
        {
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
    
}
