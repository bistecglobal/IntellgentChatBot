using System;
using System.Collections.Generic;

namespace QnABot.Models
{
    public class ConversationInformation
    {
        public ConversationInformation()
        {
            Comments = new List<string>();
        }

        public string Question { get; set; }

        public List<String> Comments { get; set; }
    }
}
