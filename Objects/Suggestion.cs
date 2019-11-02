using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System;
using DSharpPlus.Entities;
using System.Globalization;
using System.IO;

namespace Lolibase.Objects
{
    public class Suggestion
    {
        public ulong Submitter { get; private set; }
        public List<string> Links { get; private set; }
        public string Message { get; private set; }
        public Optional<Dictionary<LinkType, string>> OsuLink { get; private set; }

        public Optional<string> Image { get; set; }

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

        private Tuple<ulong, List<string>, string, Optional<Dictionary<LinkType, string>>, Optional<string>> Analize(DiscordMessage m)
        {
            string msg = m.Content;
            Regex http_sRegex = new Regex(@"(http|https):\/\/\S*", RegexOptions.IgnoreCase);
            List<Match> links = http_sRegex.Matches(msg).ToList();
            List<string> Slinks = new List<string>();
            foreach (var link in links)
            {
                Slinks.Add(link.ToString());
            }
            var osulinks = new Dictionary<LinkType, string>();
            string message = http_sRegex.Replace(msg, "");
            Optional<Dictionary<LinkType, string>> osu;
            Optional<string> img;
            if (links.Any(x => x.ToString().Contains("https://osu.ppy.sh")))
            {
                var lnks = links.FindAll(x => x.ToString().Contains("https://osu.ppy.sh"));
                foreach (var lnk in lnks)
                {
                    if (lnk.ToString().Contains("beatmapsets"))
                    {
                        osulinks.Add(LinkType.MAP, lnk.ToString());

                    }
                    else if (lnk.ToString().Contains("users"))
                    {
                        osulinks.Add(LinkType.PROFILE, lnk.ToString());
                    }
                    else if (lnk.ToString().Contains("/ss/"))
                    {
                        osulinks.Add(LinkType.SCREENSHOT, lnk.ToString());
                    }
                    else
                    {
                        osulinks.Add(LinkType.FORUMPOST, lnk.ToString());
                    }
                }
                if (osulinks.Count != 0)
                {
                    osu = new Optional<Dictionary<LinkType, string>>(osulinks);
                }
                else
                {
                    osu = new Optional<Dictionary<LinkType, string>>();
                }

            }
            else
            {
                osu = new Optional<Dictionary<LinkType, string>>();
            }
            ulong member = m.Author.Id;
            if (m.Attachments.Count != 0 && m.Attachments[0].Width != 0)
            {
                using (WebClient cl = new WebClient())
                {
                    cl.DownloadFile(m.Attachments[0].Url, $"{Directory.GetCurrentDirectory()}/tempimg.png");
                    img = new Optional<string>($"{Directory.GetCurrentDirectory()}/tempimg.png");
                }
            }
            else if (links.Any(x => IsImageUrl(x.ToString())))
            {
                var lnk = links.Find(x => IsImageUrl(x.ToString()));
                using (WebClient cl = new WebClient())
                {
                    cl.DownloadFile(lnk.ToString(), $"{Directory.GetCurrentDirectory()}/tempimg.png");
                    img = new Optional<string>($"{Directory.GetCurrentDirectory()}/tempimg.png");
                }
            }
            else
            {
                img = new Optional<string>();
            }
            return new Tuple<ulong, List<string>, string, Optional<Dictionary<LinkType, string>>, Optional<string>>(member, Slinks, message, osu, img);
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