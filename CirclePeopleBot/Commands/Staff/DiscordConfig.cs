using CirclePeopleBot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CirclePeopleBot.utils;

namespace CirclePeopleBot.Commands.Staff
{
    [Group("config"),RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    [Aliases("cfg")]
    public class DiscordConfig : BaseCommandModule 
    {
        [GroupCommand]
        public async Task MainCommand(CommandContext ctx)
        {
            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
            Embed
                .WithColor(new DiscordColor(255, 150, 202))
              .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
              .WithDescription($"Administrative Panel")
              .AddField($"Suggestion System", $"Type : **{ctx.Prefix}** config `suggestions/sug` <add/remove/list> to edit the system\n")
              .AddField($"Youtube Integration", $"Type : **{ctx.Prefix}** config `youtube/yt` to edit the integration\n")
              .AddField($"Bot Prefix", $"Type : **{ctx.Prefix}** config `botprefix` [new prefix] to edit\n")
              .WithAuthor($"Listing all Commands", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
            await ctx.RespondAsync(embed: Embed);
        }
        [Command("youtube")]
        [Aliases("yt")]
        public async Task Youtube(CommandContext ctx) {
            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
            CPInteractivity cpInteract = new CPInteractivity();
            //Example URL  : https://www.youtube.com/channel/UCiDqXBLW8zaeA34fwtH6Kmg?view_as=subscriber #ShamelessSelfPromotion
          
            DiscordChannelConverter parser = new DiscordChannelConverter();
            Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                    .WithDescription($"Please reply with the `youtube channel` (URL)")
                    .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                    .WithAuthor($"Configuration of the Youtube Integration", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
            var msg = await ctx.RespondAsync(embed: Embed);
            var input = await cpInteract.GetResponce(CPBot.Interactivity, ctx);
            string ChannelURL = input.Content;
            string ChannelID = (ChannelURL.Contains("http") ? (ChannelURL.Contains("?view_as") ? ChannelURL.Split('?')[0].Split('/').Last() : ChannelURL.Split('/').Last()) : "Invalid URL");
            Embed
                .WithColor(new DiscordColor(255, 150, 202))
                .WithDescription($"Please reply with the `discord channel` (E.g. #youtube)")
                .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                .WithAuthor($"Configuration of the Youtube Integration", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
            await msg.ModifyAsync(embed: Embed.Build());
            input = await cpInteract.GetResponce(CPBot.Interactivity, ctx);
            var Channel = await parser.ConvertAsync(input.Content, ctx);


            ////////////////////////////////////////////////////////////////////////////////////////
            var BaseChannelRequest = CPBot.YoutubeClient.Channels.List("snippet,contentDetails");
            BaseChannelRequest.Id = ChannelID;
            BaseChannelRequest.Key = CPBot.YoutubeClient.ApiKey;
            var FirstRequest = BaseChannelRequest.Execute();
            String Title =FirstRequest.Items[0].Snippet.Title;
            String ImageURL = FirstRequest.Items[0].Snippet.Thumbnails.High.Url;
            String UploadPlaylistURL = FirstRequest.Items[0].ContentDetails.RelatedPlaylists.Uploads;
            CPBot.YoutubeSystem.ChannelURL = ChannelID;
            CPBot.YoutubeSystem.UploadListURL = UploadPlaylistURL;
            CPBot.YoutubeSystem.BroadcastID = Channel.Value.Id;
            File.WriteAllText($@"{Directory.GetCurrentDirectory()}\YTConfig.json", JsonConvert.SerializeObject(CPBot.YoutubeSystem));

            Embed
                   .ClearFields()
                   .WithDescription($"YT : Channel **{Title}** [{ChannelID}] \n **DISCORD** :  {Channel.Value.Mention} set!\n\n **Sidenote : ** The YT Channel will be updated on the next cycle. [~30s]")
                   .WithColor(new DiscordColor(255, 150, 202))
                   .WithThumbnailUrl(ImageURL)
                   .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                   .WithAuthor($"Configuration of the Youtube Integration", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
            await msg.ModifyAsync(embed: Embed.Build());
        }
        #region Suggestions
        [Command("suggestions")]
        [Aliases("sug")]
        public async Task SugestionSError(CommandContext ctx)
        {
            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
            Embed
                .WithDescription($"SyntaxError : You didn't input the subcommand\n**Correct Syntax : ** {ctx.Prefix}config  `suggestions/sug` <add/remove/list>")
                .WithColor(new DiscordColor(255, 0, 0))
                .WithAuthor($"Configuration of the Suggestions System");
            await ctx.RespondAsync(embed: Embed);
        }
        [Command("suggestions")]
      
        public async Task SuggestionSystemConfig(CommandContext ctx, string operand)
        {
            if (operand.ToLowerInvariant() == "add")
            {
                CPInteractivity cpInteract = new CPInteractivity();
                DiscordChannelConverter parser = new DiscordChannelConverter();
                DiscordChannel inputChannel = null;
                DiscordChannel approvedChannel = null;
                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
                DiscordSystem mainSys = new DiscordSystem();
                Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                    .WithDescription($"Please reply with the `input channel` (The one with member input)")
                    .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                    .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
                var msg = await ctx.RespondAsync(embed: Embed);
                var input = await cpInteract.GetResponce(CPBot.Interactivity, ctx);
                var request = await parser.ConvertAsync(input.Content, ctx);
                inputChannel = request.Value;
                mainSys.inputID = inputChannel.Id;
                Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                    .WithDescription($"Please reply with the `approved channel` (The one with only staff access)")
                    .AddField($"Input Channel : ", inputChannel.Mention)
                    .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                    .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
                await msg.ModifyAsync(embed: Embed.Build());
                input = await cpInteract.GetResponce(CPBot.Interactivity, ctx);
                request = await parser.ConvertAsync(input.Content, ctx);
                approvedChannel = request.Value;
                mainSys.aprovedID = approvedChannel.Id;
                Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                    .WithDescription($"Please reply with the `name` you want to give [E.g. Standard]")
                    .AddField($"Approved Channel : ", approvedChannel.Mention)
                    .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                    .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
                await msg.ModifyAsync(embed: Embed.Build());
                input = await cpInteract.GetResponce(CPBot.Interactivity, ctx);
                mainSys.name = input.Content;
                Embed
                    .WithDescription($"Configuration of {mainSys.name} Completed!")

                    .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                    .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);


                if (!CPBot.Systems.Exists(x => x == mainSys))
                {
                    if (CPBot.Systems[0].aprovedID == 0)
                    {
                        CPBot.Systems.Remove(CPBot.Systems[0]);
                    }

                    CPBot.Systems.Add(mainSys);
                    File.WriteAllText($@"{Directory.GetCurrentDirectory()}\SystemsConfig.json", JsonConvert.SerializeObject(CPBot.Systems));
                    await msg.ModifyAsync(embed: Embed.Build());
                }
                else
                {
                    Embed.ClearFields();
                    Embed
                        .WithAuthor($"System {mainSys.name} Already exists!")
                        .WithColor(new DiscordColor(255, 0, 0));
                    await msg.ModifyAsync(embed: Embed.Build());
                }
            }
            else if (operand.ToLowerInvariant() == "remove")
            {
                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
                CPInteractivity cpInteract = new CPInteractivity();
                String listOfSystems = "";
                CPBot.Systems.ForEach(x => listOfSystems += $"{CPBot.Systems.IndexOf(x)}. {x.name}\n");
                Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                   .WithDescription($"Please reply with the `number` of the system you want to remove")
                   .AddField("List", listOfSystems)
                   .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                   .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
                var msg = await ctx.RespondAsync(embed: Embed);
                var input = await cpInteract.GetResponce(CPBot.Interactivity, ctx);
                Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                   .WithDescription($"System {CPBot.Systems[int.Parse(input.Content)].name} Removed. ")
                   .ClearFields()
                   .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                   .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
                CPBot.Systems.Remove(CPBot.Systems[int.Parse(input.Content)]);

                await msg.ModifyAsync(embed: Embed.Build());
            }
            else if (operand.ToLowerInvariant() == "list")
            {
                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
                String listOfSystems = "";
                CPBot.Systems.ForEach(x => listOfSystems += $"{CPBot.Systems.IndexOf(x)}. {x.name}\n");
                Embed
                    .WithColor(new DiscordColor(255, 150, 202))
                   .WithDescription($"List of systems currently active")
                   .AddField("List", listOfSystems)
                   .WithFooter($"Command Requested By : {ctx.Member.Username} ⌾ {DateTime.Today.DayOfWeek.ToString()} @ {DateTime.Now.ToShortTimeString()}")
                   .WithAuthor($"Configuration of the Suggestion System", "https://discord.gg/CirclePeople", ctx.Guild.IconUrl);
                var msg = await ctx.RespondAsync(embed: Embed);
            }
            
        }
        #endregion
        #region Botprefix
        [Command("botprefix")]
        public async Task BotConfigSError(CommandContext ctx)
        {

            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
            Embed
                .WithDescription($"SyntaxError : You didn't input the prefix\n**Correct Syntax : ** {ctx.Prefix}config `botprefix` [new prefix]")
                .WithColor(new DiscordColor(255,0,0))
                .WithAuthor($"Configuration of Prefix");
            await ctx.RespondAsync(embed: Embed);
        }
        [Command("botprefix")]
        public async Task BotConfig(CommandContext ctx, string prefix)
        {
            Config oldCfg = CPBot.MainCFG;
            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();
            Embed
                .WithDescription($"Prefix changed to `{prefix}`")
                .WithColor(new DiscordColor(255, 150, 202))
                .WithAuthor($"Configuration of Prefix");
            oldCfg.Prefix = new List<String> { prefix };
            File.WriteAllText($@"{Directory.GetCurrentDirectory()}\Config.json", JsonConvert.SerializeObject(oldCfg));
            CPBot.MainCFG = oldCfg;
            await ctx.RespondAsync(embed: Embed);
            
        }
        #endregion
    }
}
