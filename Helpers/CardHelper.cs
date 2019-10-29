using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using QnABot.Models;

namespace QnABot.Helpers
{
    public class CardHelper
    {
        /// <summary>
        /// Get Hero card
        /// </summary>
        /// <param name="cardTitle">Title of the card</param>
        /// <param name="prompts">List of suggested prompts</param>
        /// <returns>Message activity</returns>
        public static Activity GetHeroCardWithPrompts(string cardTitle, QnAPrompts[] prompts)
        {
            var chatActivity = Activity.CreateMessageActivity();
            var buttons = new List<CardAction>();

            var sortedPrompts = prompts.OrderBy(r => r.DisplayOrder);
            foreach (var prompt in sortedPrompts)
            {
                buttons.Add(
                    new CardAction()
                    {
                        Value = prompt.DisplayText,
                        Type = ActionTypes.ImBack,
                        Title = prompt.DisplayText
                    });
            }
            
            var plCard = new HeroCard()
            {
                Text = cardTitle,
                Subtitle = string.Empty,
                Buttons = buttons
            };

            var attachment = plCard.ToAttachment();

            chatActivity.Attachments.Add(attachment);

            return (Activity)chatActivity;
        }

        public static Activity GetQuestionPrompts(string question)
        {
            var chatActivity = Activity.CreateMessageActivity();
            var buttons = new List<CardAction>();

            buttons.Add(new CardAction() {
                Value = "1",
                Type = ActionTypes.PostBack,
                Title = "Yes"
            });

            buttons.Add(new CardAction()
            {
                Value = "0",
                Type = ActionTypes.PostBack,
                Title = "No"
            });

            var plCard = new HeroCard()
            {
                Text = question,
                Subtitle = string.Empty,
                Buttons = buttons
            };

            var attachment = plCard.ToAttachment();
            chatActivity.Attachments.Add(attachment);

            return (Activity)chatActivity;
        }

    }
}
