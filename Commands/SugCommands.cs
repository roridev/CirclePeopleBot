using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using Lolibase.Discord.Attributes;
using Lolibase.Discord.Utils;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Converters;
using Lolibase.Objects;
using System.IO;
using Newtonsoft.Json;

namespace Lolibase.Discord.Commands
{
    [Imouto, OniiSan, Group("sug"), Emoji(":white_check_mark:"),Description("Infopeople Suggestion System")]
    public class SugCommands : BaseCommandModule
    {
        [GroupCommand()]
        public async Task GroupCommand(CommandContext ctx)
        {
            await ctx.RespondAsync(null, false, EmbedBase.GroupHelpEmbed(ctx.Command));
        }
        [Command("del"),Description("Deletes a Pair")]
        public async Task DelCommand(CommandContext ctx)
        {
            var msg = await ctx.RespondAsync(embed: EmbedBase.InputEmbed("# of Pair to delete"));
            var list = await ctx.RespondAsync(embed: EmbedBase.OrderedListEmbed(Program.pairs, "Pairs"));
            var inpt = await ctx.Message.GetNextMessageAsync();
            Regex d = new Regex(@"\d+");
            var numbers = d.Split(inpt.Result.Content).ToList();
            var i = int.Parse(numbers[0]);
            var pair = Program.pairs[i];
            await list.DeleteAsync();
            await msg.ModifyAsync(embed: EmbedBase.InputEmbed($"Do you want to delete {pair.Name}? [y/n]"));
            var ipt2 = await inpt.Result.GetNextMessageAsync();
            if (ipt2.Result.Content.ToLowerInvariant().Contains("y"))
            {
                Program.pairs.Remove(pair);
                File.WriteAllText(Directory.GetCurrentDirectory() + "/Pairs.json", JsonConvert.SerializeObject(Program.pairs, Formatting.Indented));

                await msg.ModifyAsync(embed: EmbedBase.OutputEmbed($"{pair.Name} Removed with sucess."));
            }
            else
            {
                await ctx.RespondAsync(embed: EmbedBase.OutputEmbed("Command Cancelled."));
            }
        }

        [Command("add"),Description("Adds a Pair")]
        public async Task AddCommand(CommandContext ctx)
        {
            Pair p = new Pair();
            var msg = await ctx.RespondAsync(embed: EmbedBase.InputEmbed("Input Channel"));
            var inpt = await ctx.Message.GetNextMessageAsync();
            var channel = ctx.Guild.Channels.ToList().Find(x => inpt.Result.Content.Contains(x.Value.Mention));
            if (channel.Value != null)
            {
                p.InputPair = channel.Key;
                await msg.ModifyAsync(embed: EmbedBase.InputEmbed("Output Channel"));
                var outp = await inpt.Result.GetNextMessageAsync();
                var ch = ctx.Guild.Channels.ToList().Find(x => outp.Result.Content.Contains(x.Value.Mention));
                if (ch.Value != null)
                {
                    p.OutputPair = ch.Key;
                    await msg.ModifyAsync(embed: EmbedBase.InputEmbed("Name"));
                    var namaeha = await outp.Result.GetNextMessageAsync();
                    p.Name = namaeha.Result.Content;
                    if (Program.pairs.Any(x => x.InputPair == p.InputPair && x.OutputPair == p.OutputPair))
                    {
                        await ctx.RespondAsync(embed: EmbedBase.OutputEmbed($"Command Failed.\n**A Pair is already configured with** <#{p.InputPair}> *->* <#{p.OutputPair}>.\n\nName : {p.Name}"));
                    }
                    else
                    {
                        Program.pairs.Add(p);
                        File.WriteAllText(Directory.GetCurrentDirectory() + "/Pairs.json", JsonConvert.SerializeObject(Program.pairs, Formatting.Indented));
                        await msg.ModifyAsync(embed: EmbedBase.OutputEmbed($"Pair {p.Name} added sucessefully."));
                    }

                }
                else
                {
                    await ctx.RespondAsync(embed: EmbedBase.OutputEmbed($"Command Failed.\n**Output Channel Not recognized.**\n\nMake sure the cannel is mentioned like in <#{ctx.Channel.Id}>"));
                }
            }
            else
            {
                await ctx.RespondAsync(embed: EmbedBase.OutputEmbed($"Command Failed.\n**Input Channel Not recognized.**\n\nMake sure the cannel is mentioned like in <#{ctx.Channel.Id}>"));
            }
        }
    }
}