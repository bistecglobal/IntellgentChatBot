using System;
namespace QnABot.Models
{
    public class QnABotState
    {
        public int PreviousQnaId { get; set; }

        public string PreviousUserQuery { get; set; }
    }
}
