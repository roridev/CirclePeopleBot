using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace CirclePeopleBot.Objects
{
    public static class SubmissionExtension
    {
        public static Tuple<Submission,bool> GetSubmission(this DiscordMessage m)
        {
            var (description, uri) = m.Content.GetLink();
            if (uri == null)
            {
                var submission = new Submission();
                return new Tuple<Submission, bool>(submission,false);
            }
            else
            {
                var submission = new Submission {OpId = m.Author.Id,ImageLink = uri.ToString(),Description = description};
                return new Tuple<Submission, bool>(submission,true);
            }

        }

        private static Tuple<string, Uri> GetLink(this string s)
        {
            List<string> words = s.Split(' ').ToList();
            var uriIndex = -1;
            var message = "";
            Uri uri = null;
            Tuple<string, Uri> separatedMessage;
            foreach (var word in words)
            {
                if (Uri.IsWellFormedUriString(word, UriKind.Absolute))
                    uriIndex = words.IndexOf(word);
            }
            
            //On this case the last part of the message is the link, or the link is between two descriptions.
            if (words.Count == uriIndex || words.Count - uriIndex > 0 && uriIndex != -1)
            {
                Uri.TryCreate(words[uriIndex], UriKind.Absolute,out uri);
                words.Remove(words[uriIndex]);
                words.ForEach(x => message += $"{x} ");
                separatedMessage = new Tuple<string, Uri>(message,uri);
                return separatedMessage;
            }
           
            else switch (uriIndex)
            {
//              This case the link is the first part of the message, with description before it.
                case 0 when words.Count > 1:
                    Uri.TryCreate(words[uriIndex], UriKind.Absolute, out uri);
                    words.Remove(words[uriIndex]);
                    words.ForEach(x => message += $"{x} ");
                    separatedMessage = new Tuple<string, Uri>(message,uri);
                    return separatedMessage;
                //This case there is only a link and no description.
                case 0 when words.Count == 1:
                    Uri.TryCreate(words[uriIndex], UriKind.Absolute, out uri);
                    words.Remove(words[uriIndex]);
                    message = "No description was added.";
                    separatedMessage = new Tuple<string, Uri>(message,uri);
                    return separatedMessage;
                //This will trigger when there is only text
                case -1:
                    return new Tuple<string, Uri>(s,null);
                default:
                    message = s;
                    Uri.TryCreate("https://circle-people.com/", UriKind.Absolute, out uri);
                    separatedMessage = new Tuple<string, Uri>(message,uri);
                    return separatedMessage;
            }
        }
    }
}