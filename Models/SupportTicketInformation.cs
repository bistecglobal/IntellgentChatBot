using System;
using Newtonsoft.Json;

namespace QnABot.Models
{
    public class SupportTicketInformation
    {
        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }
    }
}
