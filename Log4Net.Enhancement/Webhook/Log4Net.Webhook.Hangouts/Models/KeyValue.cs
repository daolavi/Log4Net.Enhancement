using System.Runtime.Serialization;

namespace Log4Net.Webhook.Hangouts.Models
{
    [DataContract]
    public class KeyValue
    {
        [DataMember(Name = "topLabel")]
        public string TopLabel { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }
    }
}
