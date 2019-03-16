using System;
using System.Collections;

namespace Log4Net.Webhook.ElasticSearch.Models
{
    public class JsonSerializableException
    {
        public string Type { get; set; }

        public string Message { get; set; }

        public string HelpLink { get; set; }

        public string Source { get; set; }

        public int HResult { get; set; }

        public string StackTrace { get; set; }

        public IDictionary Data { get; set; }

        public JsonSerializableException InnerException { get; set; }

        public static JsonSerializableException Create(Exception ex)
        {
            if (ex == null)
                return null;

            var serializable = new JsonSerializableException
            {
                Type = ex.GetType().FullName,
                Message = ex.Message,
                HelpLink = ex.HelpLink,
                Source = ex.Source,
                StackTrace = ex.StackTrace,
                Data = ex.Data
            };

            if (ex.InnerException != null)
            {
                serializable.InnerException = Create(ex.InnerException);
            }
            return serializable;
        }
    }
}
