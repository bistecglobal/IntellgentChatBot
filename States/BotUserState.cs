using System;
using Microsoft.Bot.Builder;
using QnABot.Storages;

namespace QnABot.States
{
    public class BotUserState : UserState
    {
        public BotUserState(UserStateStorage userStateStorage)
            :base(userStateStorage)
        {
        }
    }
}
