using System;
using Microsoft.Bot.Builder;
using QnABot.Storages;

namespace QnABot.States
{
    public class BotConversationState : ConversationState
    {
        public BotConversationState(ConversationStorage conversationStorage)
            : base(conversationStorage)
        {
        }
    }
}
