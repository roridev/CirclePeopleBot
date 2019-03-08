using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirclePeopleBot.Commands.Beta
{
    public class YoutubeStuff : BaseCommandModule
    {
        [Command("getvideos")]
        public async Task GetVideos(CommandContext ctx)
        {
            var ListQuery = CPBot.YoutubeClient.Channels.List("snippet,contentDetails");
            ListQuery.Id = "UCt1GKXk_zkcBUEwAeXZ43RA";
            ListQuery.Key = CPBot.YoutubeClient.ApiKey;
            var query = ListQuery.Execute();
            var UploadPlaylist = query.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            var GetAllVideos = CPBot.YoutubeClient.PlaylistItems.List("snippet, contentDetails, status");
            GetAllVideos.PlaylistId = UploadPlaylist;
            GetAllVideos.Key = CPBot.YoutubeClient.ApiKey;
            var ListOfVideos = GetAllVideos.Execute().Items;
            PlaylistItem latestVideo = new PlaylistItem();
            foreach (var video in ListOfVideos)
            {
                if (video.Snippet.Position == 0)
                {
                    latestVideo = video;
                    break;
                }
            }
            await ctx.RespondAsync($"request completed, latest video from circle people = http://youtu.be/{latestVideo.ContentDetails.VideoId}/");
        }
    }
}
