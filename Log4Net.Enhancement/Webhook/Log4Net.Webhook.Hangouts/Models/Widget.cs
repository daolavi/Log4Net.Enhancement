using System.Runtime.Serialization;

namespace Log4Net.Webhook.Hangouts.Models
{
    [DataContract]
    public class Widget
    {
        [DataMember(Name = "keyValue")]
        public KeyValue KeyValue { get; set; }
    }
}
