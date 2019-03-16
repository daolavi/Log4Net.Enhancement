using Log4Net.Webhook.Extensions;
using Log4Net.Webhook.Hangouts.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Log4Net.Webhook.Hangouts
{
    public class HangoutsClient
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        private static readonly List<Task> Tasks = new List<Task>();

        /// <summary>
        /// Post a message to GoogleChat.
        /// </summary>
        public static void PostMessageAsync(string url, Message mesage)
        {
            try
            {
                var jsonData = mesage.JsonSerializeObject();
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var task = Client.PostAsync(url, content);
                Tasks.Add(task);
            }
            catch (Exception)
            {
                //  Ignore error.
            }
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
