using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace QnABot.Cards
{
    public class WelcomeDialog
    {
        private readonly string cardPath;

        public WelcomeDialog()
        {
            cardPath = Path.Combine("Cards", "welcomeMessage.json");
        }

        public IMessageActivity Create()
        {
            return MessageFactory.Attachment(CreateAdaptiveCardAttachment());
        }

        private Attachment CreateAdaptiveCardAttachment()
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
