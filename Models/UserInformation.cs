using System;
using Newtonsoft.Json;

namespace QnABot.Models
{
    public class UserInformation
    {
        public UserInformation()
        {
        }

        [JsonProperty("user")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string UserEmail { get; set; }
    }
}
