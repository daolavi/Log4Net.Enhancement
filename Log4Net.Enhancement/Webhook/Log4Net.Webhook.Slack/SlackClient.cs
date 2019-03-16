using Log4Net.Webhook.Extensions;
using Log4Net.Webhook.Slack.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Log4Net.Webhook.Slack
{
    public static class SlackHttpClient
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        private static readonly List<Task> Tasks = new List<Task>();

        /// <summary>
        /// Post a message to Slack.
        /// </summary>
        /// <param name="text">The text of the message.</param>
        /// <param name="url">Slack webhook url</param>
        /// <param name="username">If provided, overrides the existing username.</param>
        /// <param name="channel">If provided, overrides the existing channel.</param>
        /// <param name="iconUrl"></param>
        /// <param name="iconEmoji"></param>
        /// <param name="attachments">Optional collection of attachments.</param>
        /// <param name="linknames">Whether or not to link names in the Slack message.</param>
        public static void PostMessageAsync(string text,
                                            string url,
                                            string username = null,
                                            string channel = null,
                                            string iconUrl = null,
                                            string iconEmoji = null,
                                            List<Attachment> attachments = null,
                                            bool linknames = false)
        {
            var payload = BuildPayload(text, username, channel, iconUrl, iconEmoji, attachments, linknames);
            var data = payload.JsonSerializeObject();
            PostPayloadAsync(url, data);
        }

        /// <summary>
        /// Posts a payload to Slack.
        /// </summary>
        private static void PostPayloadAsync(string url, string json)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var task = Client.PostAsync(url, content);
                Tasks.Add(task);
            }
            catch (Exception)
            {
                //  Ignore error.
            }
        }

        private static Payload BuildPayload(string text,
                                            string username,
                                            string channel,
                                            string iconUrl,
                                            string iconEmoji,
                                            List<Attachment> attachments = null,
                                            bool linknames = false)
        {
            username = string.IsNullOrEmpty(username) ? string.Empty : username;
            channel = string.IsNullOrEmpty(channel) ? string.Empty : channel;
            iconUrl = string.IsNullOrEmpty(iconUrl) ? string.Empty : iconUrl;
            iconEmoji = string.IsNullOrEmpty(iconEmoji) ? string.Empty : iconEmoji;

            var payload = new Payload
            {
                Channel = channel,
                Username = username,
                IconUrl = iconUrl,
                IconEmoji = iconEmoji,
                Text = text,
                Attachments = attachments,
                LinkNames = Convert.ToInt32(linknames)
            };

            return payload;
        }

        /// <summary>
        /// This tasks was created to support Console application.
        /// In case console terminates itself without waiting message to be sent to Slack api
        /// This method will trigger by "LogManager.Shutdown();" 
        /// </summary>
        public static void WaitAll()
        {
            Task.WaitAll(Tasks.ToArray());
        }
    }
}
