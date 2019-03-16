using System.Runtime.Serialization;

namespace Log4Net.Webhook.Slack.Models
{
    /// <summary>
    /// Fields are displayed in a table on the message.
    /// </summary>
    [DataContract]
    public class Field
    {
        public Field(string title, string value = null, bool Short = false)
        {
            Title = title;
            Value = value;
            this.Short = Short;
        }

        /// <summary>
        /// The title may not contain markup and will be escaped for you; required.
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Text value of the field. May contain standard message markup and must be escaped as normal; may be multi-line.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Optional flag indicating whether the <paramref name="Value"/> is short enough to be displayed side-by-side with other values.
        /// </summary>
        [DataMember(Name = "short")]
        public bool Short { get; set; }
    }
}
