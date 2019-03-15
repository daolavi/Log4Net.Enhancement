using System.Runtime.Serialization;

namespace Log4Net.Webhook.Hangouts.Models
{
    [DataContract]
    public class Message
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "cards")]
        public Card Card { get; set; }
    }
}
