using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Log4Net.Webhook.Slack.Models
{
    /// <summary>
    /// The payload to send to Stack, which will be serialized to JSON before POSTing.
    /// </summary>
    [DataContract]
    public class Payload
    {
        [DataMember(Name = "channel")]
        public string Channel { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "icon_url")]
        public string IconUrl { get; set; }

        [DataMember(Name = "icon_emoji")]
        public string IconEmoji { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "attachments")]
        public List<Attachment> Attachments { get; set; }

        [DataMember(Name = "link_names")]
        public int LinkNames { get; set; }
    }
}
