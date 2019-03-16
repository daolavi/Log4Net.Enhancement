using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Log4Net.Webhook.Slack.Models
{
    /// <summary>
    /// It is possible to create more richly-formatted messages using Attachments.
    /// https://api.slack.com/docs/attachments
    /// https://api.slack.com/docs/messages
    /// </summary>
    [DataContract]
    public class Attachment
    {
        public Attachment(string fallback)
        {
            Fallback = fallback;
            MarkdownIn = new List<string> { "fields" };
        }

        /// <summary>
        /// Required text summary of the attachment that is shown by clients that understand attachments but choose not to show them.
        /// </summary>
        [DataMember(Name = "fallback")]
        public string Fallback { get; set; }

        /// <summary>
        /// Optional text that should appear above the formatted data.
        /// </summary>
        [DataMember(Name = "pretext")]
        public string PreText { get; set; }

        /// <summary>
        /// Optional text that should appear within the attachment.
        /// </summary>
        [DataMember(Name = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Can either be one of 'good', 'warning', 'danger', or any hex color code.
        /// </summary>
        [DataMember(Name = "color")]
        public string Color { get; set; }

        /// <summary>
        /// Fields are displayed in a table on the message.
        /// </summary>
        [DataMember(Name = "fields")]
        public List<Field> Fields { get; set; }

        [DataMember(Name = "mrkdwn_in")]
        public List<string> MarkdownIn { get; private set; }
    }
}
