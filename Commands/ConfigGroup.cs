using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CirclePeopleBot.Objects;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;

namespace CirclePeopleBot.Commands
{
    [Group("config"),Staff]
    [Aliases("cfg")]
    public class ConfigGroup : BaseCommandModule
    {
        [Command("suggestion")]
        [Aliases("sug")]
        public async Task Suggestion(CommandContext ctx)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder
                .AddField("Available Operations","add - Adds a pair to the system\ndel - Removes a pair from the system\nlist - Lists all pairs")
                .WithDescription($"Command Usage : {ctx.Prefix}`cfg sug <operation>`")
                .WithAuthor("Config : Suggestion")
                .WithColor(DiscordColor.Yellow);
            await ctx.RespondAsync(embed: embedBuilder);
        }
        
        [Command("suggestion")]
        public async Task SuggestionCorrect(CommandContext ctx, string operation)
        {
            Pair p = new Pair();
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            DiscordChannelConverter converter = new DiscordChannelConverter();
            if (operation.ToLowerInvariant() == "add")
            {
                embedBuilder
                    .WithDescription("Type the `Input Channel`")
                    .WithAuthor("Suggestion : Pairs")
                    .WithColor(DiscordColor.Green);
                var message =  await ctx.RespondAsync(embed: embedBuilder);
                InteractivityResult<DiscordMessage> query = await ctx.Message.GetNextMessageAsync();
                var input = query.Result;
                var fromChannel = (DiscordChannel) await converter.ConvertAsync(input.Content, ctx);
                embedBuilder
                    .WithDescription("Type the `Approved Channel`")
                    .AddField("Input Channel", fromChannel.Mention);
                await message.ModifyAsync(embed: embedBuilder.Build());
                query = await input.GetNextMessageAsync();
                input = query.Result;
                var toChannel = (DiscordChannel) await converter.ConvertAsync(input.Content, ctx);
                embedBuilder
                    .AddField("Approved Channel",toChannel.Mention)
                    .WithDescription("Type the `name of this pair`");
                await message.ModifyAsync(embed: embedBuilder.Build());
                query = await input.GetNextMessageAsync();
                input = query.Result;
                embedBuilder
                    .WithDescription($"Configuration of {input.Content} finished.")
                    .WithAuthor($"Suggestion : Pairs ({input.Content})");
                await message.ModifyAsync(embed: embedBuilder.Build());
                p.FromChannel = fromChannel.Id;
                p.ToChannel = toChannel.Id;
                p.PairName = input.Content;
                if (Program.Pairs.Find(x => x.FromChannel == p.FromChannel && x.ToChannel == p.ToChannel) == null)
                {
                    Program.Pairs.Add(p);
                    File.WriteAllText($@"{Directory.GetCurrentDirectory()}/Pairs.json",JsonConvert.SerializeObject(Program.Pairs, Formatting.Indented));
                }

            }
            else if (operation.ToLowerInvariant() == "del")
            {
                var allPairs = "";
                embedBuilder.WithAuthor("Suggestion : Pairs");
                if (Program.Pairs.Count > 0)
                {
                    Program.Pairs.ForEach(x => allPairs += $"[{Program.Pairs.IndexOf(x)}] {x.PairName} | [<#{x.FromChannel}>] -> <#{x.ToChannel}>\n");
                    embedBuilder
                        .WithColor(DiscordColor.Green)
                        .AddField("[Pair #] [Name] [Input Channel] [Approved Channel]", allPairs)
                        .WithDescription("Please type the `number of the pair` you want to delete, or use `cancel` for cancelling the command\n**THIS CAN'T BE UNDONE**");
                    var message = await ctx.RespondAsync(embed: embedBuilder);
                    InteractivityResult<DiscordMessage> query = await ctx.Message.GetNextMessageAsync();
                    var input = query.Result;
                    if (input.Content.ToLowerInvariant() != "cancel")
                    {
                        p = Program.Pairs[Convert.ToInt32(input.Content)];
                        embedBuilder
                            .WithDescription($"Pair `{p.PairName} deleted`")
                            .ClearFields()
                            .WithAuthor($"Suggestion : Pairs ({p.PairName})");
                        Program.Pairs.Remove(p);
                        File.WriteAllText($@"{Directory.GetCurrentDirectory()}/Pairs.json",JsonConvert.SerializeObject(Program.Pairs, Formatting.Indented));
                        await message.ModifyAsync(embed: embedBuilder.Build());
                    }
                    
                }
                
                

            }
            else if (operation.ToLowerInvariant() == "list")
            {
                var allPairs = "";
                embedBuilder.WithAuthor("Suggestion : Pairs");
                if (Program.Pairs.Count > 0)
                {
                    Program.Pairs.ForEach(x =>
                        allPairs +=
                            $"[{Program.Pairs.IndexOf(x)}] {x.PairName} | [<#{x.FromChannel}>] -> <#{x.ToChannel}>\n");
                    embedBuilder
                        .WithColor(DiscordColor.Green)
                        .AddField("[Pair #] [Name] [Input Channel] [Approved Channel]", allPairs)
                        .WithDescription("List of all active pairs");
                    await ctx.RespondAsync(embed: embedBuilder);
                }
                else
                {
                    embedBuilder
                        .WithColor(DiscordColor.Green)
                        .WithDescription($"No pairs could be found. Add one using {ctx.Prefix}`cfg sug add`");
                    await ctx.RespondAsync(embed: embedBuilder);
                }
            }
        }
    }
}