using System;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Bot.Schema;

namespace QnABot.Helpers
{
    public static class ChannelData
    {
        public static bool IsMsTeams(this IMessageActivity messageActivity, out TeamsChannelData channelData)
        {
            return messageActivity.TryGetChannelData(out channelData);
        }
    }
}
