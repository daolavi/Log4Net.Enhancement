using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Log4Net.Webhook.Extensions
{
    public static class StringExtensions
    {
        public static string Expand(this string text)
        {
            return text != null ? Environment.ExpandEnvironmentVariables(text) : null;
        }

        public static IEnumerable<string> SplitOn(this string text, int numChars)
        {
            var splitOnPattern = new Regex($@"(?<line>.{{1,{numChars}}})([\r\n]|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return splitOnPattern.Matches(text).OfType<Match>().Select(m => m.Groups["line"].Value);
        }

        public static string FormatString(this log4net.Layout.ILayout layout, log4net.Core.LoggingEvent loggingEvent)
        {
            using (var writer = new StringWriter())
            {
                layout.Format(writer, loggingEvent);
                return writer.ToString();
            }
        }

        public static string JsonSerializeObject(this object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
