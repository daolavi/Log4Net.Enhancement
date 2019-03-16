using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using log4net.Core;
using log4net.Util;

namespace Log4Net.Webhook.Extensions
{
    public static class MethodExtensions
    {
        public static void Do<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
            {
                action(item);
            }
        }

        public static string With(this string self, params object[] args)
        {
            return string.Format(self, args);
        }

        public static IEnumerable<KeyValuePair<string, string>> Properties(this LoggingEvent self)
        {
            return self.GetProperties().AsPairs();
        }

        public static bool Contains(this StringDictionary self, string key)
        {
            return self.ContainsKey(key) && !self[key].IsNullOrEmpty();
        }

        public static bool ToBool(this string self)
        {
            return bool.Parse(self);
        }

        /// <summary>
        /// Take the full connection string and break it into is constituent parts
        /// </summary>
        /// <param name="self">The connection string itself</param>
        /// <returns>A dictionary of all the parts</returns>
        public static StringDictionary ConnectionStringParts(this string self)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = self.Replace("{", "\"").Replace("}", "\"")
            };

            var parts = new StringDictionary();
            foreach (string key in builder.Keys)
            {
                parts[key] = Convert.ToString(builder[key]);
            }
            return parts;
        }

        static IEnumerable<KeyValuePair<string, string>> AsPairs(this ReadOnlyPropertiesDictionary self)
        {
            return self.GetKeys().Select(key => new KeyValuePair<string, string>(key, self[key].ToStringOrNull()));
        }

        private static string ToStringOrNull(this object self)
        {
            return self?.ToString();
        }

        private static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
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
