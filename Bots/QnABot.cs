// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
using QnABot.States;
using Newtonsoft.Json;

namespace QnABot.Bots
{
    public class QnABot : ActivityHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QnABot> _logger;
        private readonly IQnAService _qnAService;
        private readonly WelcomeCard _welcomeDialog;
        private readonly SupportTicketCard _supportTicketCard;
        private readonly BotConversationState _botConversationState;
        private readonly BotUserState _botUserState;

        private StepInformation stepInformation;

        private readonly int _minConfidenceScore;

        public QnABot(IConfiguration configuration,
            ILogger<QnABot> logger,
            IQnAService qnAService,
            WelcomeCard welcomeDialog,
            SupportTicketCard supportTicketCard,
            BotConversationState botConversationState,
            BotUserState botUserState)
        {
            _configuration = configuration;
            _logger = logger;
            _qnAService = qnAService;
            _welcomeDialog = welcomeDialog;
            _supportTicketCard = supportTicketCard;

            _botConversationState = botConversationState;
            _botUserState = botUserState;

            int.TryParse(configuration["MinConfidenceScore"], out _minConfidenceScore);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(_welcomeDialog.Create(), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _botUserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            stepInformation = await GetConversationStep(turnContext);

            switch (stepInformation.Step)
            {
                case ChatStep.UserInformation:
                case ChatStep.None:
                    CaptureUserInformation(turnContext, cancellationToken);
                    break;
                case ChatStep.ChatInformation:
                    CaptureChatInformation(turnContext, cancellationToken);
                    break;
                case ChatStep.SupportTicket:
                    CaptureSupportTicket(turnContext, cancellationToken);
                    break;
                default:
                    break;
            }

            //QnAResult[] qnaResults = await _qnAService.QueryQnAServiceAsync(turnContext.Activity.Value.ToString(), new QnABotState(), QnAMakerEndpoint);

            //if (qnaResults.Any())
            //{
            //    QnAResult highestRankedResult = qnaResults.OrderByDescending(x => x.Score).First();
            //    var answer = highestRankedResult.Answer;

            //    QnAPrompts[] prompts = highestRankedResult.Context?.Prompts;
                
            //    if (prompts == null || prompts.Length < 1)
            //    {
            //        if (highestRankedResult.Score <= _minConfidenceScore)
            //            await turnContext.SendActivityAsync(_supportTicketCard.Create("how to fix the blue screen error?", "this is a test comment"), cancellationToken);
            //        else
            //            await turnContext.SendActivityAsync(answer, cancellationToken: cancellationToken);
            //    }
            //    else
            //    {
            //        await turnContext.SendActivityAsync(CardHelper.GetHeroCardWithPrompts(answer, prompts), cancellationToken: cancellationToken);
            //    }
            //}
            //else
            //{
            //    await turnContext.SendActivityAsync(_supportTicketCard.Create("how to fix the blue screen error?", "this is a test comment"), cancellationToken);
            //}
        }

        private QnAMakerEndpoint QnAMakerEndpoint
        {
            get
            {
                return new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                    EndpointKey = _configuration["QnAAuthKey"],
                    Host = GetHostname()
                };
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

        private async Task<StepInformation> GetConversationStep(ITurnContext turnContext)
        {
            var stepInformationStateAccessors = _botConversationState.CreateProperty<StepInformation>(nameof(StepInformation));

            return await stepInformationStateAccessors.GetAsync(turnContext, () => new StepInformation());
        }

        private async Task CaptureUserInformation(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userInformationStateAccessors = _botConversationState.CreateProperty<UserInformation>(nameof(UserInformation));

            var userInformation = await userInformationStateAccessors.GetAsync(turnContext, () => new UserInformation());

            var inputValue = stepInformation.UserInputType == InputType.Text ? turnContext.Activity.Text : turnContext.Activity.Value.ToString();

            var jsonData = JsonConvert.DeserializeObject<UserInformation>(inputValue);

            userInformation.UserName = jsonData.UserName;
            userInformation.UserEmail = jsonData.UserEmail;

            validateUserInput(turnContext, jsonData, cancellationToken);

            stepInformation.Step = ChatStep.ChatInformation;
            await turnContext.SendActivityAsync($"Welcome {userInformation.UserName}. Please enter your question to proceed");
        }

        private async Task CaptureChatInformation(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            QnAResult[] qnaResults = await _qnAService.QueryQnAServiceAsync(turnContext.Activity.Text, new QnABotState(), QnAMakerEndpoint);

            if (qnaResults.Any())
            {
                QnAResult highestRankedResult = qnaResults.OrderByDescending(x => x.Score).First();
                var answer = highestRankedResult.Answer;

                QnAPrompts[] prompts = highestRankedResult.Context?.Prompts;

                if (prompts == null || prompts.Length < 1)
                {
                    if (highestRankedResult.Score <= _minConfidenceScore)
                        await turnContext.SendActivityAsync(_supportTicketCard.Create("how to fix the blue screen error?", "this is a test comment"), cancellationToken);
                    else
                        await turnContext.SendActivityAsync(answer, cancellationToken: cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(CardHelper.GetHeroCardWithPrompts(answer, prompts), cancellationToken: cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync(_supportTicketCard.Create("how to fix the blue screen error?", "this is a test comment"), cancellationToken);
            }
        }

        private void CaptureSupportTicket(ITurnContext turnContext, CancellationToken cancellationToken)
        {

        }

        private void validateUserInput(ITurnContext turnContext, UserInformation userInformation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userInformation.UserName))
            {
                stepInformation.UserInputType = InputType.Text;
                turnContext.SendActivityAsync("Please enter your name", cancellationToken: cancellationToken);
            }
            else if (string.IsNullOrEmpty(userInformation.UserEmail))
            {
                stepInformation.UserInputType = InputType.Text;
                turnContext.SendActivityAsync("Please enter your email", cancellationToken: cancellationToken);
            }
        }



    }
}
