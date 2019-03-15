using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Log4Net.Webhook.Hangouts.Models
{
    [DataContract]
    public class Card
    {
        [DataMember(Name = "sections")]
        public List<Section> Sections { get; set; }
    }
}
