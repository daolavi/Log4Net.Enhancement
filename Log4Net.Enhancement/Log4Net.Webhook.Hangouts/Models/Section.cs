using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Log4Net.Webhook.Hangouts.Models
{
    [DataContract]
    public class Section
    {
        [DataMember(Name = "widgets")]
        public List<Widget> Widgets { get; set; }
    }
}
