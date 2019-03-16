using log4net.Appender;
using Log4Net.Webhook.Extensions;
using Log4Net.Webhook.Slack.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Log4Net.Webhook.Slack
{
    /// <summary>
    /// Reference: https://github.com/jonfreeland/Log4Slack
    /// </summary>
    public class SlackAppender : AppenderSkeleton
    {
        private readonly Process currentProcess = Process.GetCurrentProcess();
        private readonly List<Mapping> mappings = new List<Mapping>();

        /// <summary>
        /// Slack webhook URL, with token.
        /// </summary>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Slack channel to send log events to.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Username to post to Slack as.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The URL of the icon to use, if any.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// The name of the Emoji icon to use, if any.
        /// </summary>
        public string IconEmoji { get; set; }

        /// <summary>
        /// Indicates whether or not to include additional details in message attachments.
        /// </summary>
        public bool AddAttachment { get; set; }

        /// <summary>
        /// Indicates whether or not to include the exception traces as fields on message attachments.
        /// Requires AddAttachment be true.
        /// </summary>
        public bool AddExceptionTraceField { get; set; }

        /// <summary>
        /// Indicates whether or not to append the logger name to the Stack username.
        /// </summary>
        public bool UsernameAppendLoggerName { get; set; }

        /// <summary>
        /// The optional proxy configuration for outgoing slack posts
        /// </summary>
        public string Proxy { get; set; }

        /// <summary>
        /// Whether to tell Slack API to automatically link @mentions
        /// </summary>
        public bool LinkNames { get; set; }

        /// <summary>
        /// Specify program's environment
        /// </summary>
        public string ProgramEnvironment { get; set; }

        //public SlackClient slackClient;
        //public SlackHttpClient httpClient;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            var attachments = new List<Attachment>();

            if (AddAttachment)
            {
                // Set fallback string
                var theAttachment = new Attachment(string.Format("[{0}] {1} in {2} on {3}",
                                                                 loggingEvent.Level.DisplayName,
                                                                 loggingEvent.LoggerName,
                                                                 currentProcess.ProcessName,
                                                                 Environment.MachineName));

                // Determine attachment color
                switch (loggingEvent.Level.DisplayName.ToLowerInvariant())
                {
                    case "warn":
                        theAttachment.Color = "warning";
                        break;
                    case "error":
                    case "fatal":
                        theAttachment.Color = "danger";
                        break;
                }

                //override colors from config if available
                var mapping = mappings?.FirstOrDefault(m => m.level.Equals(loggingEvent.Level.DisplayName, StringComparison.InvariantCultureIgnoreCase));
                if (mapping != null)
                {
                    var color = Color.FromName(mapping.backColor);
                    var hex = color.IsKnownColor ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : mapping.backColor;
                    theAttachment.Color = !string.IsNullOrEmpty(hex) ? hex : theAttachment.Color;
                }

                // Add attachment fields
                theAttachment.Fields = new List<Field>
                {
                        new Field("Process", currentProcess.ProcessName, true),
                        new Field("Machine", Environment.MachineName, true),
                        new Field("Environment", ProgramEnvironment, true),
                };
                if (!UsernameAppendLoggerName) theAttachment.Fields.Insert(0, new Field("Logger", loggingEvent.LoggerName, true));

                // Add exception fields if exception occurred
                var exception = loggingEvent.ExceptionObject;
                if (exception != null)
                {
                    theAttachment.Fields.Insert(0, new Field("Exception Type", exception.GetType().Name, true));
                    if (AddExceptionTraceField && !string.IsNullOrWhiteSpace(exception.StackTrace))
                    {
                        var parts = exception.StackTrace.SplitOn(1990).ToArray(); // Split call stack into consecutive fields of ~2k characters
                        for (int idx = parts.Length - 1; idx >= 0; idx--)
                        {
                            var name = "Exception Trace" + (idx > 0 ? $" {idx + 1}" : null);
                            theAttachment.Fields.Insert(0, new Field(name, "```" + parts[idx].Replace("```", "'''") + "```"));
                        }
                    }

                    theAttachment.Fields.Insert(0, new Field("Exception Message", exception.Message));
                }

                attachments.Add(theAttachment);
            }

            var formattedMessage = (Layout != null ? Layout.FormatString(loggingEvent) : loggingEvent.RenderedMessage);
            var username = Username.Expand() + (UsernameAppendLoggerName ? " - " + loggingEvent.LoggerName : null);

            SlackHttpClient.PostMessageAsync(formattedMessage,
                                             WebhookUrl.Expand(),
                                             username,
                                             Channel.Expand(),
                                             IconUrl.Expand(),
                                             IconEmoji.Expand(),
                                             attachments,
                                             LinkNames);
        }

        /// <summary>
        /// Override OnClose() to wait the async tasks which sending message to Slack api
        /// This tasks was created to support Console application.
        /// In case console app terminates itself without waiting message to be sent to Slack api
        /// This method will trigger by "LogManager.Shutdown();" 
        /// </summary>
        protected override void OnClose()
        {
            SlackHttpClient.WaitAll();
            base.OnClose();
        }
    }
}
