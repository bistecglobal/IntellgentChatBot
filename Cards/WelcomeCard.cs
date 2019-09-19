using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using QnABot.Helpers;

namespace QnABot.Cards
{
    public class WelcomeCard
    {
        private readonly string cardPath;

        public WelcomeCard()
        {
            cardPath = Path.Combine("Cards", "WelcomeMessage.json");
        }

        public IMessageActivity Create()
        {
            return MessageFactory.Attachment(CreateAdaptiveCardAttachment());
        }

        public Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCardJson = File.ReadAllText(cardPath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
