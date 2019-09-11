//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License.

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.AI.QnA;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Schema;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using QnABot.Helpers;
//using QnABot.Models;

//namespace QnABot.Dialogs
//{
//    public class MainDialog : ComponentDialog
//    {
//        private readonly IConfiguration _configuration;
//        protected readonly ILogger Logger;
//        private readonly IQnAService _qnAService;

//        private QnAMakerEndpoint _qnAMakerEndpoint;

//        // Dependency injection uses this constructor to instantiate MainDialog
//        public MainDialog(ILogger<MainDialog> logger,
//            IConfiguration configuration,
//            IQnAService qnAService)
//            : base(nameof(MainDialog))
//        {
//            Logger = logger;
//            _configuration = configuration;
//            _qnAService = qnAService;

//            AddDialog(new TextPrompt(nameof(TextPrompt)));
//            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
//            {
//                IntroStepAsync,
//                ActStepAsync,
//                FinalStepAsync,
//            }));

//            // The initial child Dialog to run.
//            InitialDialogId = nameof(WaterfallDialog);
//        }

//        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            Console.WriteLine("");
//            //if (!_luisRecognizer.IsConfigured)
//            //{
//            //    await stepContext.Context.SendActivityAsync(
//            //        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

//            //    return await stepContext.NextAsync(null, cancellationToken);
//            //}

//            //// Use the text provided in FinalStepAsync or the default if it is the first time.
//            //var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?";
//            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

//            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
//        }

//        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            //if (!_luisRecognizer.IsConfigured)
//            //{
//            //    // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
//            //    return await stepContext.BeginDialogAsync(nameof(FlightBookingDialog), new BookingDetails(), cancellationToken);
//            //}

//            //// Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
//            //var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
//            //switch (luisResult.TopIntent().intent)
//            //{
//            //    case Intent.BookFlight:
//            //        var flightBooking = new FlightBooking(luisResult);
//            //        await ShowWarningForUnsupportedCities(stepContext.Context, flightBooking, cancellationToken);

//            //        // Initialize BookingDetails with any entities we may have found in the response.
//            //        var bookingDetails = new BookingDetails()
//            //        {
//            //            // Get destination and origin from the composite entities arrays.
//            //            Destination = flightBooking.To,
//            //            Origin = flightBooking.From,
//            //            TravelDate = flightBooking.TravelDate,
//            //        };

//            //        // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
//            //        return await stepContext.BeginDialogAsync(nameof(FlightBookingDialog), bookingDetails, cancellationToken);

//            //    case Intent.BookTaxi:
//            //        var taxiBooking = new TaxiBooking(luisResult);

//            //        // Initialize BookingDetails with any entities we may have found in the response.
//            //        var taxiBookingDetails = new BookingDetails()
//            //        {
//            //            // Get destination and origin from the composite entities arrays.
//            //            Destination = taxiBooking.To,
//            //            Origin = taxiBooking.From,
//            //            TravelDate = taxiBooking.TravelDate,
//            //        };

//            //        return await stepContext.BeginDialogAsync(nameof(TaxiBookingDialog), taxiBookingDetails, cancellationToken);

//            //    case Intent.OnBoard:
//            //        var onboarding = new OnBoarding(luisResult);

//            //        var onBoardingDetails = new OnBoardingDetails()
//            //        {
//            //            EmployeeName = onboarding.EmployeeName,
//            //            PersonalEmail = onboarding.PersonalEmail,
//            //            OfficialEmail = onboarding.OfficialEmail,
//            //        };

//            //        return await stepContext.BeginDialogAsync(nameof(OnBoardingDialog), onBoardingDetails, cancellationToken);
//            //    case Intent.None:
//            //        var response = await _bookingQnAMaker.GetAnswerAsync(stepContext.Context);

//            //        if (response != null && response.Length > 0)
//            //            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
//            //        else
//            //            await stepContext.Context.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);

//            //        break;
//            //    default:
//            //        // Catch all for unhandled intents
//            //        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way";
//            //        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
//            //        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
//            //        break;
//            //}

//            //return await stepContext.NextAsync(null, cancellationToken);
//        }

//        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
//            // the Result here will be null.
//            //if (stepContext.Result is BookingDetails result)
//            //{
//            //    // Now we have all the booking details call the booking service.

//            //    // If the call to the booking service was successful tell the user.

//            //    var timeProperty = new TimexProperty(result.TravelDate);
//            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
//            //    var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
//            //    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
//            //    await stepContext.Context.SendActivityAsync(message, cancellationToken);
//            //}

//            //// Restart the main dialog with a different message the second time around
//            //var promptMessage = "What else can I do for you?";
//            //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
//        }

//        private async Task FindAnswer(ITurnContext turnContext, string question, CancellationToken cancellationToken)
//        {
//            var qnaMakerEndPoint = GetQnAMakerEndpoint;
//            QnAResult[] qnaResults = await _qnAService.QueryQnAServiceAsync(turnContext.Activity.Text, new QnABotState(), qnaMakerEndPoint);

//            if (qnaResults.Any())
//            {
//                // Get result by highest confidence
//                QnAResult highestRankedResult = qnaResults.OrderByDescending(x => x.Score).First();
//                string answer = highestRankedResult.Answer;

//                QnAPrompts[] prompts = highestRankedResult.Context?.Prompts;

//                if (prompts == null || prompts.Length < 1)
//                {
//                    await turnContext.SendActivityAsync(answer, cancellationToken: cancellationToken);
//                }
//                else
//                {
//                    await turnContext.SendActivityAsync(CardHelper.GetHeroCardWithPrompts(answer, prompts), cancellationToken: cancellationToken);
//                }
//            }
//            else
//            {
//                await turnContext.SendActivityAsync(CardHelper.GetQuestionPrompts("You got what you want, or shall I create a ticket?"), cancellationToken: cancellationToken);
//            }
//        }

//        private QnAMakerEndpoint GetQnAMakerEndpoint
//        {
//            get
//            {
//                if (_qnAMakerEndpoint == null)
//                {
//                    _qnAMakerEndpoint = new QnAMakerEndpoint
//                    {
//                        KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
//                        EndpointKey = _configuration["QnAAuthKey"],
//                        Host = GetHostname()
//                    };
//                }

//                return _qnAMakerEndpoint;
//            }
//        }

//        private string GetHostname()
//        {
//            var hostname = _configuration["QnAEndpointHostName"];
//            if (!hostname.StartsWith("https://", System.StringComparison.Ordinal))
//            {
//                hostname = string.Concat("https://", hostname);
//            }

//            if (!hostname.EndsWith("/qnamaker", System.StringComparison.Ordinal))
//            {
//                hostname = string.Concat(hostname, "/qnamaker");
//            }

//            return hostname;
//        }
//    }
//}
