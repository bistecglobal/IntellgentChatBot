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
using System;

namespace QnABot.Bots
{
    public class QnABot : ActivityHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QnABot> _logger;
        private readonly IQnAService _qnAService;
        private readonly WelcomeCard _welcomeDialog;
        private readonly SupportTicketCard _supportTicketCard;
        private readonly UserFeedbackCard _userFeedbackCard;
        private readonly BotConversationState _botConversationState;
        private readonly int _minConfidenceScore;
        private readonly Incident _incident;

        private StepInformation stepInformation;

        public QnABot(IConfiguration configuration,
            ILogger<QnABot> logger,
            IQnAService qnAService,
            WelcomeCard welcomeDialog,
            SupportTicketCard supportTicketCard,
            BotConversationState botConversationState,
            UserFeedbackCard userFeedbackCard,
            Incident incident)
        {
            _configuration = configuration;
            _logger = logger;
            _qnAService = qnAService;
            _welcomeDialog = welcomeDialog;
            _supportTicketCard = supportTicketCard;
            _botConversationState = botConversationState;
            _incident = incident;
            _userFeedbackCard = userFeedbackCard;

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

            await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            stepInformation = await GetConversationStep(turnContext);

            switch (stepInformation.Step)
            {
                case ChatStep.UserInformation:
                case ChatStep.None:
                    await CaptureUserInformation(turnContext, cancellationToken);
                    break;
                case ChatStep.ChatInformation:
                    await CaptureChatInformation(turnContext, cancellationToken);
                    break;
                case ChatStep.UserFeedback:
                    await CaptureUserFeedback(turnContext, cancellationToken);
                    break;
                case ChatStep.SupportTicket:
                    await CaptureSupportTicketInformation(turnContext, cancellationToken);
                    break;
                default:
                    break;
            }
        }

        private async Task<StepInformation> GetConversationStep(ITurnContext turnContext)
        {
            var stepInformationStateAccessors = _botConversationState.CreateProperty<StepInformation>(nameof(StepInformation));

            return await stepInformationStateAccessors.GetAsync(turnContext, () => new StepInformation());
        }

        private async Task CaptureUserInformation(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userInformation = await GetUserInformation(turnContext);

            var inputValue = turnContext.Activity.Value == null ? turnContext.Activity.Text : turnContext.Activity.Value.ToString();

            var jsonData = JsonConvert.DeserializeObject<UserInformation>(inputValue);

            userInformation.UserName = jsonData.UserName;
            userInformation.UserEmail = jsonData.UserEmail;

            validateUserInput(turnContext, jsonData, cancellationToken);

            stepInformation.Step = ChatStep.ChatInformation;
            await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await turnContext.SendActivityAsync($"Welcome {userInformation.UserName}. Please enter your question to proceed");
        }

        private async Task CaptureChatInformation(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var conversationInformation = await GetConversationInformation(turnContext);

            QnAResult[] qnaResults = await _qnAService.QueryQnAServiceAsync(turnContext.Activity.Text, new QnABotState(), QnAMakerEndpoint);

            if (qnaResults.Any())
            {
                QnAResult highestRankedResult = qnaResults.OrderByDescending(x => x.Score).First();
                var answer = highestRankedResult.Answer;

                QnAPrompts[] prompts = highestRankedResult.Context?.Prompts;

                if (prompts == null || prompts.Length < 1)
                {
                    if (highestRankedResult.Score <= _minConfidenceScore)
                    {
                        stepInformation.Step = ChatStep.UserFeedback;
                        conversationInformation.Question = turnContext.Activity.Text;
                        await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                        await turnContext.SendActivityAsync(_userFeedbackCard.Create(), cancellationToken);
                    }
                    else
                    {
                        conversationInformation.Comments.Add(answer);
                        await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                        await turnContext.SendActivityAsync(answer, cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    conversationInformation.Comments.Add(answer);
                    await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                    await turnContext.SendActivityAsync(CardHelper.GetHeroCardWithPrompts(answer, prompts), cancellationToken: cancellationToken);
                }
            }
        }

        private async Task CaptureUserFeedback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var conversationInformation = await GetConversationInformation(turnContext);

            var inputValue = turnContext.Activity.Value == null ?
                turnContext.Activity.Text : turnContext.Activity.Value.ToString();

            var feedbackInformation = JsonConvert.DeserializeObject<UserFeedbackInformation>(inputValue);

            if (feedbackInformation.Feedback.Equals("create"))
            {
                stepInformation.Step = ChatStep.SupportTicket;

                await _botConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                var activity = _supportTicketCard.Create(conversationInformation.Question, String.Join(", ", conversationInformation.Comments.ToArray()));

                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }

        private async Task CaptureSupportTicketInformation(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var supportInformation = await GetSupportInformation(turnContext);

            var inputValue = turnContext.Activity.Value == null ? turnContext.Activity.Text : turnContext.Activity.Value.ToString();

            var jsonData = JsonConvert.DeserializeObject<SupportTicketInformation>(inputValue);

            supportInformation.Question = jsonData.Question;
            supportInformation.Comments = jsonData.Comments;

            var response = await _incident.Create(supportInformation);

            if (!string.IsNullOrEmpty(response))
                await turnContext.SendActivityAsync($"New ticket created in our incident tracker {response}", cancellationToken: cancellationToken);
            else
                await turnContext.SendActivityAsync("Incident creation failed", cancellationToken: cancellationToken);
        }

        private async Task<UserInformation> GetUserInformation(ITurnContext turnContext)
        {
            var userInformationStateAccessors = _botConversationState.CreateProperty<UserInformation>(nameof(UserInformation));
            var userInformation = await userInformationStateAccessors.GetAsync(turnContext, () => new UserInformation());

            return userInformation;
        }

        private async Task<ConversationInformation> GetConversationInformation(ITurnContext turnContext)
        {
            var conversationInformationStateAccessors = _botConversationState.CreateProperty<ConversationInformation>(nameof(ConversationInformation));
            var conversationInformation = await conversationInformationStateAccessors.GetAsync(turnContext, () => new ConversationInformation());

            return conversationInformation;
        }

        private async Task<SupportTicketInformation> GetSupportInformation(ITurnContext turnContext)
        {
            var supportInformationStateAccessors = _botConversationState.CreateProperty<SupportTicketInformation>(nameof(SupportTicketInformation));
            var supportInformation = await supportInformationStateAccessors.GetAsync(turnContext, () => new SupportTicketInformation());

            return supportInformation;
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

        private void validateSupportInput(ITurnContext turnContext, SupportTicketInformation supportTicketInformation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(supportTicketInformation.Question))
            {
                stepInformation.UserInputType = InputType.Text;
                turnContext.SendActivityAsync("Please enter your question", cancellationToken: cancellationToken);
            }
            else if (string.IsNullOrEmpty(supportTicketInformation.Comments))
            {
                stepInformation.UserInputType = InputType.Text;
                turnContext.SendActivityAsync("Please enter comments", cancellationToken: cancellationToken);
            }
        }

        private QnAMakerEndpoint QnAMakerEndpoint
        {
            get
            {
                return new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                    EndpointKey = _configuration["QnAAuthKey"],
                    Host = _configuration["QnAEndpointHostName"]
                };
            }
        }

    }
}
