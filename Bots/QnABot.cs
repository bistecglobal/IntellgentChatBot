// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QnABot.Cards;
using QnABot.Models;
using QnABot.Helpers;
using Microsoft.Bot.Builder.AI.QnA;
using System.Linq;

namespace QnABot.Bots
{
    public class QnABot : ActivityHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QnABot> _logger;
        private readonly IQnAService _qnAService;
        private readonly WelcomeDialog _welcomeDialog;

        private QnAMakerEndpoint _qnAMakerEndpoint;

        public QnABot(IConfiguration configuration,
            ILogger<QnABot> logger,
            IQnAService qnAService,
            WelcomeDialog welcomeDialog)
        {
            _configuration = configuration;
            _logger = logger;
            _qnAService = qnAService;
            _welcomeDialog = welcomeDialog;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(_welcomeDialog.Create(), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            QnAResult[] qnaResults = await _qnAService.QueryQnAServiceAsync(turnContext.Activity.Text, new QnABotState(), GetQnAMakerEndpoint);

            if (qnaResults.Any())
            {
                // Get result by highest confidence
                QnAResult highestRankedResult = qnaResults.OrderByDescending(x => x.Score).First();
                string answer = highestRankedResult.Answer;

                QnAPrompts[] prompts = highestRankedResult.Context?.Prompts;

                if (prompts == null || prompts.Length < 1)
                {
                    await turnContext.SendActivityAsync(answer, cancellationToken: cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(CardHelper.GetHeroCardWithPrompts(answer, prompts), cancellationToken: cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync(CardHelper.GetQuestionPrompts("You got what you want, or shall I create a ticket?"), cancellationToken: cancellationToken);
            }
        }

        private QnAMakerEndpoint GetQnAMakerEndpoint
        {
            get
            {
                if (_qnAMakerEndpoint == null)
                {
                    _qnAMakerEndpoint = new QnAMakerEndpoint
                    {
                        KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                        EndpointKey = _configuration["QnAAuthKey"],
                        Host = GetHostname()
                    };
                }

                return _qnAMakerEndpoint;
            }
        }

        private string GetHostname()
        {
            var hostname = _configuration["QnAEndpointHostName"];
            if (!hostname.StartsWith("https://", System.StringComparison.Ordinal))
            {
                hostname = string.Concat("https://", hostname);
            }

            if (!hostname.EndsWith("/qnamaker", System.StringComparison.Ordinal))
            {
                hostname = string.Concat(hostname, "/qnamaker");
            }

            return hostname;
        }
    }
}
