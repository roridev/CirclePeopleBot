using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CirclePeopleBot.Objects
{
    public static class PairExtension
    {
        public static async Task<bool> Initialize(this Pair p, DiscordClient client)
        {
            try
            {
                var fromChannel = await client.GetChannelAsync(p.FromChannel);
                List<DiscordMessage> lastMessage = (await fromChannel.GetMessagesAsync()).ToList();
                foreach (var message in lastMessage)
                {
                    
                    var (submission, status) = message.GetSubmission();
                    DiscordEmoji upvoteEmoji = await fromChannel.Guild.FetchEmoji(Program.MainCfg.UpvoteId) ?? DiscordEmoji.FromUnicode(client,"👍");
                    DiscordEmoji downvoteEmoji = await fromChannel.Guild.FetchEmoji(Program.MainCfg.DownvoteId) ?? DiscordEmoji.FromUnicode(client, "👎") ;

                    if (!status) continue;
                    var upvotes = new List<DiscordUser>();
                    var downvotes = new List<DiscordUser>();
                    IReadOnlyList<DiscordReaction> reactions = message.Reactions;
                    if (reactions.Count == 0)
                    {
                        await message.CreateReactionAsync(upvoteEmoji);
                        await message.CreateReactionAsync(downvoteEmoji);
                    }
                    else
                    {
                        foreach (var reaction in reactions)
                        {
                            if (reaction.Emoji == upvoteEmoji)
                            {
                                upvotes = (await message.GetReactionsAsync(upvoteEmoji)).ToList();
                            }
                            else if (reaction.Emoji == downvoteEmoji)
                            {
                                downvotes = (await message.GetReactionsAsync(downvoteEmoji)).ToList();
                            }
                            else
                            {
                                IReadOnlyList<DiscordUser> reactionsToDelete =
                                    await message.GetReactionsAsync(reaction.Emoji);
                                foreach (var user in reactionsToDelete)
                                {
                                    await message.DeleteReactionAsync(reaction.Emoji, user);
                                }
                            }
                        }

                        if (upvotes.Any(user => (fromChannel.Guild.GetMemberAsync(user.Id).Result).IsStaff(fromChannel)))
                        {
                            var toChannel = await client.GetChannelAsync(p.ToChannel);
                            var op = await toChannel.Guild.GetMemberAsync(submission.OpId);
                            var embedBuilder = new DiscordEmbedBuilder();
                            var opName = op.Nickname ?? op.Username;
                            embedBuilder
                                .WithDescription($"About :\n{submission.Description}")
                                .WithColor(new DiscordColor(150, 202, 255))
                                .WithFooter($"Submitted by : {opName}#{op.Discriminator}")
                                .WithAuthor("Score approved",submission.ImageLink);
                            if (submission.ImageLink.Contains(".png")||submission.ImageLink.Contains(".jpg"))
                            {
                                embedBuilder.WithImageUrl(submission.ImageLink);
                            }
                            else
                            {
                                embedBuilder.AddField("Link", submission.ImageLink);
                            }

                            await toChannel.SendMessageAsync(embed: embedBuilder.Build());
                            await message.DeleteAsync();
                        }
                        if (downvotes.Any(user => (fromChannel.Guild.GetMemberAsync(user.Id).Result).IsStaff(fromChannel)))
                        {
                            await message.DeleteAsync();
                        }

                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
        }
    }
}