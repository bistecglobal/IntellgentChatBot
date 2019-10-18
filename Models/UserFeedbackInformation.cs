using System;
using Newtonsoft.Json;

namespace QnABot.Models
{
    public class UserFeedbackInformation
    {
        [JsonProperty("operation")]
        public string Feedback { get; set; }
    }

    public enum UserFeedback
    {
        CreateSupportTicket,
        DontCreateSupportTicket
    }
}
