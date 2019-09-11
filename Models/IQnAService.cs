using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA;

namespace QnABot.Models
{
    public interface IQnAService
    {
        Task<QnAResult[]> QueryQnAServiceAsync(string query, QnABotState qnAcontext, QnAMakerEndpoint qnAMakerEndpoint);
    }
}
