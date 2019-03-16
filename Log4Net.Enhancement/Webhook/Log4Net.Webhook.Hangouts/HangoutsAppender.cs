using log4net.Appender;
using Log4Net.Webhook.Extensions;
using Log4Net.Webhook.Hangouts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Log4Net.Webhook.Hangouts
{
    public class HangoutsAppender : AppenderSkeleton
    {
        private readonly Process currentProcess = Process.GetCurrentProcess();

        /// <summary>
        /// Slack webhook URL, with token.
        /// </summary>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Specify program's environment
        /// </summary>
        public string ProgramEnvironment { get; set; }


        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            var message = new Message
            {
                Text = string.Empty,
                Card = new Card()
                {
                    Sections = new List<Section>
                    {
                        new Section
                        {
                            Widgets = new List<Widget>()
                        }
                    }
                }
            };

            var text = new StringBuilder();
            var formattedMessage = (Layout != null ? Layout.FormatString(loggingEvent) : loggingEvent.RenderedMessage);
            text.Append(formattedMessage);
            var exception = loggingEvent.ExceptionObject;
            if (exception != null)
            {
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    text.AppendLine(exception.StackTrace);
                }

                message.Card.Sections[0].Widgets.Add(new Widget
                {
                    KeyValue = new KeyValue
                    {
                        TopLabel = "Exception Type",
                        Content = exception.GetType().Name,
                    }
                }
                );
            }
            message.Text = text.ToString();

            message.Card.Sections[0].Widgets.AddRange(new List<Widget>
            {
                new Widget
                {
                    KeyValue = new KeyValue
                    {
                        TopLabel = "Process",
                        Content = currentProcess.ProcessName,
                    },
                },
                new Widget
                {
                    KeyValue = new KeyValue
                    {
                        TopLabel = "Machine",
                        Content = Environment.MachineName,
                    },
                },
                new Widget
                {
                    KeyValue = new KeyValue
                    {
                        TopLabel = "Environment",
                        Content = ProgramEnvironment,
                    },
                },
            });

            HangoutsClient.PostMessageAsync(WebhookUrl.Expand(), message);
        }

        /// <summary>
        /// Override OnClose() to wait the async tasks which sending message to Slack api
        /// This tasks was created to support Console application.
        /// In case console app terminates itself without waiting message to be sent to Slack api
        /// This method will trigger by "LogManager.Shutdown();" 
        /// </summary>
        protected override void OnClose()
        {
            HangoutsClient.WaitAll();
            base.OnClose();
        }
    }
}
