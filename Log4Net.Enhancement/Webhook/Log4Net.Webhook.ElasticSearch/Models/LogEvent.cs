using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using System.Runtime.Serialization;
using Log4Net.Webhook.Extensions;

namespace Log4Net.Webhook.ElasticSearch.Models
{
    /// <summary>
    /// Primary object which will get serialized into a json object to pass to ES. Deviating from CamelCase
    /// class members so that we can stick with the build in serializer and not take a dependency on another lib. ES
    /// expects fields to start with lowercase letters.
    /// </summary>
    [DataContract]
    public class LogEvent
    {
        public LogEvent()
        {
            Properties = new Dictionary<string, string>();
        }

        [DataMember(Name = "timeStamp")]
        public string TimeStamp { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "messageObject")]
        public object MessageObject { get; set; }

        [DataMember(Name = "exception")]
        public object Exception { get; set; }

        [DataMember(Name = "loggerName")]
        public string LoggerName { get; set; }

        [DataMember(Name = "domain")]
        public string Domain { get; set; }

        [DataMember(Name = "identity")]
        public string Identity { get; set; }

        [DataMember(Name = "level")]
        public string Level { get; set; }

        [DataMember(Name = "className")]
        public string ClassName { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "lineNumber")]
        public string LineNumber { get; set; }

        [DataMember(Name = "fullInfo")]
        public string FullInfo { get; set; }

        [DataMember(Name = "methodName")]
        public string MethodName { get; set; }

        [DataMember(Name = "fix")]
        public string Fix { get; set; }

        [DataMember(Name = "properties")]
        public IDictionary<string, string> Properties { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "threadName")]
        public string ThreadName { get; set; }

        [DataMember(Name = "hostName")]
        public string HostName { get; set; }

        public static IEnumerable<LogEvent> CreateMany(IEnumerable<LoggingEvent> loggingEvents)
        {
            return loggingEvents.Select(Create).ToArray();
        }

        static LogEvent Create(LoggingEvent loggingEvent)
        {
            var logEvent = new LogEvent
            {
                LoggerName = loggingEvent.LoggerName,
                Domain = loggingEvent.Domain,
                Identity = loggingEvent.Identity,
                ThreadName = loggingEvent.ThreadName,
                UserName = loggingEvent.UserName,
                TimeStamp = loggingEvent.TimeStamp.ToUniversalTime().ToString("O"),
                Exception = loggingEvent.ExceptionObject == null ? new object() : JsonSerializableException.Create(loggingEvent.ExceptionObject),
                Message = loggingEvent.RenderedMessage,
                Fix = loggingEvent.Fix.ToString(),
                HostName = Environment.MachineName,
                Level = loggingEvent.Level?.DisplayName
            };

            // Added special handling of the MessageObject since it may be an exception. 
            // Exception Types require specialized serialization to prevent serialization exceptions.
            if (loggingEvent.MessageObject != null && loggingEvent.MessageObject.GetType() != typeof(string))
            {
                if (loggingEvent.MessageObject is Exception exception)
                    logEvent.MessageObject = JsonSerializableException.Create(exception);
                else
                    logEvent.MessageObject = loggingEvent.MessageObject;
            }
            else
            {
                logEvent.MessageObject = new object();
            }

            if (loggingEvent.LocationInformation != null)
            {
                logEvent.ClassName = loggingEvent.LocationInformation.ClassName;
                logEvent.FileName = loggingEvent.LocationInformation.FileName;
                logEvent.LineNumber = loggingEvent.LocationInformation.LineNumber;
                logEvent.FullInfo = loggingEvent.LocationInformation.FullInfo;
                logEvent.MethodName = loggingEvent.LocationInformation.MethodName;
            }
            foreach (var property in loggingEvent.Properties())
                logEvent.Properties.Add(property.Key, property.Value);
            logEvent.Properties.Add("@timestamp", loggingEvent.TimeStamp.ToUniversalTime().ToString("O"));

            return logEvent;
        }
    }
}
