using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System;
using DSharpPlus.Entities;
using System.Globalization;

namespace Lolibase.Objects
{
    public class Suggestion
    {
        public ulong Submitter { get; private set; }
        public List<string> Links { get; private set; }
        public string Message { get; private set; }
        public Optional<string> OsuLink { get; private set; }

        public Optional<byte[]> Image { get; set; }

        public Suggestion(DiscordMessage message)
        {
            if (!String.IsNullOrWhiteSpace(message.Content))
            {
                var (submitter, links, text, osulink, image) = Analize(message);
                this.Submitter = submitter;
                this.Links = links;
                this.OsuLink = osulink;
                this.Message = text;
                this.Image = image;
            }
        }

        private Tuple<ulong, List<string>, string, Optional<string>, Optional<byte[]>> Analize(DiscordMessage m)
        {
            string msg = m.Content;
            Regex http_sRegex = new Regex(@"(http|https):\/\/\S*", RegexOptions.IgnoreCase);
            List<Match> links = http_sRegex.Matches(msg).ToList();
            List<string> Slinks = new List<string>();
            foreach(var link in links)
            {
                Slinks.Add(link.ToString());
            }
            string message = http_sRegex.Replace(msg, "");
            Optional<string> osu;
            Optional<byte[]> img;
            if (links.Any(x => x.ToString().Contains("https://osu.ppy.sh")))
            {
                var lnk = links.Find(x => x.ToString().Contains("https://osu.ppy.sh"));
                osu = new Optional<string>(lnk.ToString());
            }
            else
            {
                osu = new Optional<string>();
            }
            ulong member = m.Author.Id;
            if (m.Attachments.Count != 0 && m.Attachments[0].Width != 0)
            {
                using (WebClient cl = new WebClient())
                {
                    img = new Optional<byte[]>(cl.DownloadData(m.Attachments[0].Url));
                }
            }
            else if (links.Any(x => IsImageUrl(x.ToString())))
            {
                var lnk = links.Find(x => IsImageUrl(x.ToString()));
                using (WebClient cl = new WebClient())
                {
                    img = new Optional<byte[]>(cl.DownloadData(lnk.ToString()));
                }
            }
            else
            {
                img = new Optional<byte[]>();
            }
            return new Tuple<ulong, List<string>, string, Optional<string>, Optional<byte[]>>(member, Slinks, message, osu, img);
        }

        bool IsImageUrl(string URL)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                           .StartsWith("image/");
            }
        }
    }
}