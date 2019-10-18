using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace QnABot.Cards
{
    public class UserFeedbackCard
    {
        private readonly string cardPath;

        public UserFeedbackCard()
        {
            cardPath = Path.Combine("Cards", "userFeedbackForm.json");
        }

        public IMessageActivity Create()
        {
            var adaptiveCardJson = File.ReadAllText(cardPath);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return MessageFactory.Attachment(adaptiveCardAttachment);
        }
    }
}
