using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using QnABot.Helpers;

namespace QnABot.Cards
{
    public class SupportTicketCard
    {
        private readonly string cardPath;

        public SupportTicketCard()
        {
            cardPath = Path.Combine("Cards", "supportTicketForm.json");
        }

        public IMessageActivity Create(string question, string questionData)
        {
            var adaptiveCardJson = File.ReadAllText(cardPath);
            adaptiveCardJson = adaptiveCardJson.Replace("<%question%>", question);
            adaptiveCardJson = adaptiveCardJson.Replace("<%comments%>", questionData);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return MessageFactory.Attachment(adaptiveCardAttachment);
        }

    }
}
