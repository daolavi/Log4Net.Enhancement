using Log4Net.Webhook.ElasticSearch.Models;
using Log4Net.Webhook.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Log4Net.Webhook.ElasticSearch
{
    public interface IEsHttpClient
    {
        void PostBulk(IEnumerable<LogEvent> items, CancellationToken cancellationToken);
    }

    public class EsHttpClient : IEsHttpClient
    {

        const string ContentType = "application/json";
        private readonly System.Uri _uri;

        public EsHttpClient(string connectionString)
        {
            _uri = Models.Uri.For(connectionString);
        }

        /// <summary>
        /// Post the events to the ElasticSearch _bulk API for faster inserts
        /// </summary>
        public void PostBulk(IEnumerable<LogEvent> items, CancellationToken cancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(_uri.UserInfo))
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(_uri.UserInfo)));
                var postBody = new StringBuilder();
                foreach (var item in items)
                {
                    postBody.AppendLine("{\"index\" : {} }");
                    postBody.AppendLine(item.JsonSerializeObject());
                }
                var content = new StringContent(postBody.ToString(), Encoding.UTF8, ContentType);
                Task.WaitAll(httpClient.PostAsync(_uri, content, cancellationToken));
                //await httpClient.PostAsync(_uri, content, cancellationToken);
            }
        }
    }
}
