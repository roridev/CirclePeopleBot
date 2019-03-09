using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using netCore_CirclePeopleBot.Configs;
using netCore_CirclePeopleBot.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace netCore_CirclePeopleBot
{
    class nc_CPBot
    {
    /* Bot Properties
    * All public acessible properties
    */
        public static nc_CPBot Instance { get; set; }
        public static CommandsNextExtension CommandsNext { get; set; }
        public static InteractivityExtension Interactivity { get; set; }
        public static MainCFG BotConfig { get; set; }
        public static DiscordClient Client { get; set; }
        public static List<VotingChannelPair> VotingSystem { get; set; }
        public static YouTubeService YoutubeClient = new YouTubeService();
        public static YoutubeConfig YoutubeConfig { get; set; }

    /* Main Method
     * 
     */
        static void Main(string[] args)
        {


            if (File.Exists($@"{Directory.GetCurrentDirectory()}\YTConfig.json"))
            {
                YoutubeConfig = JsonConvert.DeserializeObject<YoutubeConfig>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\YTConfig.json"));

            }
            else
            {
                YoutubeConfig = new YoutubeConfig { ChannelID = "0", LastVideoID = "0", BroadcastID = 0 ,ApiKey = "AIzaSyD7i-7a5dTIvxsy20j6McqxE1FXczlepVQ" };

            }

            YoutubeClient = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = "AIzaSyD7i-7a5dTIvxsy20j6McqxE1FXczlepVQ",
                ApplicationName = "CirclePeopleRetriever"
            });

            if (File.Exists($@"{Directory.GetCurrentDirectory()}\SystemsConfig.json"))
            {
                VotingSystem = JsonConvert.DeserializeObject<List<VotingChannelPair>>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\SystemsConfig.json"));

            }
            else
            {
                VotingSystem = new List<VotingChannelPair> { new VotingChannelPair { InputChannelID = 0, ApprovedChannelID = 0, PairName = "INITIAL PLACEHOLDER, DONT WORRY IT WILL DELETE ITSELF WHEN YOU CREATE YOUR FIRST SYSTEM" } };

            }
            if (!File.Exists($@"{Directory.GetCurrentDirectory()}\Config.json"))
            {
                //Configuration
                Console.WriteLine($"Welcome to the Configuration Screen for the bot.\n" +
                    $"Please Type the token that will be used for the connection and press enter :\n");
                String CfgToken = Console.ReadLine();
                Console.WriteLine($"\n\nToken set!\nType the prefix/prefixes (comma [,] separated) that will be used by the bot :\n");
                String buffer = Console.ReadLine();
                List<String> Prefixes = (buffer.Contains(",") ? buffer.Split(',').ToList() : new List<string> { buffer });
                if (Prefixes.Count == 1)
                {
                    Console.WriteLine($"\n\nPrefix {Prefixes[0]} set! Configuration Done.\n");

                }
                else
                {
                    String streamlined = "";
                    Prefixes.ForEach(x => streamlined += $"{x} , ");
                    Console.WriteLine($"\n\nPrefixes {streamlined}. set! Configuration Done.\n");

                }

                MainCFG newCfg = new MainCFG { EnableDMs= true, Prefixes = Prefixes, Token = CfgToken };
                File.WriteAllText($@"{Directory.GetCurrentDirectory()}\Config.json", JsonConvert.SerializeObject(newCfg));
                BotConfig = JsonConvert.DeserializeObject<MainCFG>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Config.json"));


                Instance = new nc_CPBot();
                Instance.StartAsync().GetAwaiter().GetResult();




            }
            else
            {
                BotConfig= JsonConvert.DeserializeObject<MainCFG>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Config.json"));

                Instance = new nc_CPBot();
                Instance.StartAsync().GetAwaiter().GetResult();

            }

        }

        /* Bot Constructor
         * DiscordAPI + Bot Configuration
         */
        public nc_CPBot()
        {
            Client = new DiscordClient
                (
                    new DiscordConfiguration
                    {
                        UseInternalLogHandler = true,
                        Token = BotConfig.Token,
                        TokenType = TokenType.Bot
                    }
                );

            Interactivity = Client.UseInteractivity
                (
                    new InteractivityConfiguration
                    {
                        PaginationBehavior = TimeoutBehaviour.DeleteReactions,
                        PaginationTimeout = TimeSpan.FromMinutes(20),
                        Timeout = TimeSpan.FromMinutes(30)
                    }
                );

            CommandsNext = Client.UseCommandsNext
                (
                    new CommandsNextConfiguration
                    {
                        PrefixResolver = ResolvePrefixAsync,
                        StringPrefixes = BotConfig.Prefixes,
                        EnableDms = BotConfig.EnableDMs,
                        EnableDefaultHelp = false,
                        EnableMentionPrefix = true
                    }
                );

            CommandsNext.RegisterCommands(Assembly.GetEntryAssembly());

            //DiscordApi EventHandlers
            Client.MessageCreated += MessageCreated;
            Client.MessageReactionAdded += ReactionAdded;

            //Timer
            _ = Task.Run(async () => {
                await VideoLookup();
                System.Timers.Timer timer = new System.Timers.Timer
                {
                    Interval = 60 * 1000,
                };
                timer.Elapsed += async delegate
                {
                    await VideoLookup();
                };
                timer.Start();

            });
        

    }

        async Task ReactionAdded(MessageReactionAddEventArgs e)
        {

            if (VotingSystem[0].InputChannelID != 0 && VotingSystem.Find(x => x.InputChannelID == e.Channel.Id) != null)
            {
                DiscordGuild Guild = e.Channel.Guild;
                VotingChannelPair System = VotingSystem.Find(x => x.InputChannelID == e.Channel.Id);
                DiscordMember Member = await Guild.GetMemberAsync(e.User.Id);
                if (Member.IsStaff(e.Channel))
                {
                    if (e.Emoji.Name == "👍")
                    {
                        DiscordEmbedBuilder TransferedEmbed = new DiscordEmbedBuilder();
                        DiscordChannel ApprovedChannel = e.Channel.Guild.GetChannel(System.ApprovedChannelID);
                        Submission Sub = await e.Message.GetSubmissionByMessage();
                        if (Sub.ImageURL != "Invalid Submission")
                        {

                            string FileExtension = Sub.ImageURL.Split('.').ToList().Last();
                            string localFilename = $@"{Directory.GetCurrentDirectory()}\temp.{FileExtension}";
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile(Sub.ImageURL, localFilename);
                            }
                            TransferedEmbed
                           .WithFooter($"Post by : {Sub.Author.Username}#{Sub.Author.Discriminator}")
                           .WithColor(new DiscordColor(255, 150, 202))
                           .WithDescription(Sub.SuggestionText)
                           .WithAuthor($"Post Approved");
                            var sentMessage = await ApprovedChannel.SendFileAsync(localFilename, embed: TransferedEmbed, content: "@here");
                            await sentMessage.CreateReactionAsync(DiscordEmoji.FromUnicode(nc_CPBot.Client, "👍"));
                            await sentMessage.CreateReactionAsync(DiscordEmoji.FromUnicode(nc_CPBot.Client, "👎"));
                            await e.Message.DeleteAsync();

                        }
                    }
                    else if (e.Emoji.Name == "👎")
                    {
                        await e.Message.DeleteAsync();
                    }
                }
            }
        }

        async Task MessageCreated(MessageCreateEventArgs e)
        {
            if (VotingSystem[0].InputChannelID != 0 && VotingSystem.Find(x => x.InputChannelID == e.Channel.Id) != null)
            {
                DiscordMember member = await e.Channel.Guild.GetMemberAsync(e.Message.Author.Id);
                if (!member.IsBot)
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(nc_CPBot.Client, "👍"));
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(nc_CPBot.Client, "👎"));
                }
            }
        }

        async Task<int> ResolvePrefixAsync(DiscordMessage msg)
        {
            int passed = -1;
            foreach (string prefix in BotConfig.Prefixes)
            {
                passed = msg.GetStringPrefixLength(prefix);
            }
            return passed;
        }

        private async static Task VideoLookup()
        {
            if (nc_CPBot.YoutubeConfig.ChannelID != "0")
            {
                var Request = nc_CPBot.YoutubeClient.PlaylistItems.List("snippet, contentDetails, status");
                Request.Key = nc_CPBot.YoutubeClient.ApiKey;
                Request.PlaylistId = nc_CPBot.YoutubeConfig.UploadPlaylistID;
                Request.MaxResults = 2;
                var Responce = Request.Execute();
                var LatestVideos = Responce.Items;
                foreach (var video in LatestVideos)
                {
                    if (video.Snippet.ResourceId.VideoId != nc_CPBot.YoutubeConfig.LastVideoID && nc_CPBot.YoutubeConfig.BroadcastID != 0 && video.Snippet.Position == 0)
                    {
                        var broadcast = await Client.GetChannelAsync(nc_CPBot.YoutubeConfig.BroadcastID);
                        DiscordEmbedBuilder videoEmbed = new DiscordEmbedBuilder();
                        videoEmbed
                            .WithImageUrl($"{video.Snippet.Thumbnails.High.Url}")
                            .WithDescription($"{(video.Snippet.PublishedAt.HasValue ? $" Published at : {video.Snippet.PublishedAt?.ToShortDateString()} @ {video.Snippet.PublishedAt?.ToShortTimeString()}" : ".")}")
                            .WithAuthor($" New Video : {video.Snippet.Title}", $"http://youtu.be/{video.Snippet.ResourceId.VideoId}")
                            .WithColor(new DiscordColor(255, 150, 202));
                        nc_CPBot.YoutubeConfig.LastVideoID = video.Snippet.ResourceId.VideoId;
                        File.WriteAllText($@"{Directory.GetCurrentDirectory()}\YTConfig.json", JsonConvert.SerializeObject(nc_CPBot.YoutubeConfig));
                        await broadcast.SendMessageAsync(embed: videoEmbed);
                    }

                }
            }
        }

        public async Task StartAsync()
        {
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
