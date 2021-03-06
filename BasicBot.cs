// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// See https://github.com/microsoft/botbuilder-samples for a more comprehensive list of samples.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Snow;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class BasicBot : IBot
    {
        // Supported LUIS Intents
        public const string GreetingIntent = "Greeting";
        public const string CancelIntent = "Cancel";
        public const string HelpIntent = "Help";
        public const string NoneIntent = "None";
        public const string ThatsALotIntent = "ThatsAlot";
        public const string WhatIMissedIntent = "WhatIMissed";
        public const string WhatNextIntent = "WhatNext";
        public const string MeetingsIntent = "Meetings";
        public const string MeetingsWithBossIntent = "MeetingsWithBoss";
        public const string ServiceNowIntent = "ServiceNow";
        public const string ServiceNowPendingApprovalsIntent = "ServiceNowPendingApprovals";
        public const string ShowDetailsIntent = "ShowDetails";
        public const string EmailsIntent = "Emails";
        public const string EmailsFromBossIntent = "EmailsFromBoss";
       

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "BasicBotLuisApplication";

        private readonly BotServices _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicBot"/> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        public BasicBot(BotServices services, ILoggerFactory loggerFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            // Verify LUIS configuration.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

        }


        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            if (activity.Type == ActivityTypes.Message)
            {
                if(activity.Text == "")
                {
                    await turnContext.SendActivityAsync("Got blank txt");
                    // Replay to the activity we received with an activity.
                    var reply = activity.CreateReply();

                    // Cards are sent as Attackments in the Bot Framework.
                    // So we need to create a list of attachments on the activity.
                    reply.Attachments = new List<Attachment>();
                    //reply.Attachments.Add(CreateAdaptiveCardAttachment());
                    //reply.Attachments.Add(GetHeroCard().ToAttachment());


                    //var welcomeCard = CreateInitialGreetingCardAttachment();
                    //var serviceNowCard = CreateServiceNowCardAttachment();
                    //reply.Attachments.Add(welcomeCard);
                    //reply.Attachments.Add(serviceNowCard);

                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    // Perform a call to LUIS to retrieve results for the current activity message.
                    var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(turnContext, cancellationToken).ConfigureAwait(false);

                    // If any entities were updated, treat as interruption.
                    // For example, "no my name is tony" will manifest as an update of the name to be "tony".
                    var topScoringIntent = luisResults?.GetTopScoringIntent();

                    var topIntent = topScoringIntent.Value.intent;
                    var reply = activity.CreateReply();
                    switch (topIntent)
                    {
                        case GreetingIntent:
                            await turnContext.SendActivityAsync("Hello!");
                            // Replay to the activity we received with an activity.
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            //reply.Attachments.Add(CreateAdaptiveCardAttachment());
                            //reply.Attachments.Add(GetHeroCard().ToAttachment());

                            var welcomeCard = CreateInitialGreetingCardAttachment();
                            reply.Attachments.Add(welcomeCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case HelpIntent:
                            await turnContext.SendActivityAsync("Let me try to provide some help.");
                            await turnContext.SendActivityAsync("I understand greetings, being asked for help, or being asked to cancel what I am doing.");
                            break;
                        case WhatIMissedIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            //var serviceNowCard = CreateServiceNowSummaryCardAttachment();
                            //reply.Attachments.Add(serviceNowCard);
                            var emailSummaryCard = CreateEMailSummaryCardAttachment();
                            reply.Attachments.Add(emailSummaryCard);
                            //var meetingSummaryCard = CreateMeetingSummaryCardAttachment();
                            //reply.Attachments.Add(meetingSummaryCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case ServiceNowIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var serviceNowCard = CreateServiceNowSummaryCardAttachment();
                            reply.Attachments.Add(serviceNowCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case ServiceNowPendingApprovalsIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var serviceNowPendingApprovalsCard = CreateServiceNowPendingApprovalsCardAttachment();
                            reply.Attachments.Add(serviceNowPendingApprovalsCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case EmailsIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var emailsSummaryCard = CreateEMailSummaryCardAttachment();
                            reply.Attachments.Add(emailsSummaryCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case EmailsFromBossIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var emailsFromBossCard = CreateEMailFromBossCardAttachment();
                            reply.Attachments.Add(emailsFromBossCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case MeetingsIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var meetingsCard = CreateMeetingSummaryCardAttachment();
                            reply.Attachments.Add(meetingsCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case MeetingsWithBossIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var meetingsWithBossCard = CreateMeetingsWithBossCardAttachment();
                            reply.Attachments.Add(meetingsWithBossCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case ThatsALotIntent:
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            var thatsALotCard = CreateEncouragementCardAttachment();
                            reply.Attachments.Add(thatsALotCard);
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case ShowDetailsIntent:
                            await turnContext.SendActivityAsync("Hello!");
                            // Replay to the activity we received with an activity.
                            reply = activity.CreateReply();
                            reply.Attachments = new List<Attachment>();
                            reply.Attachments.Add(GetShowDetailsCard().ToAttachment());
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        case CancelIntent:
                            await turnContext.SendActivityAsync("I have nothing to cancel.");
                            break;
                        case NoneIntent:
                        default:
                            // Help or no intent identified, either way, let's provide some help.
                            // to the user
                            await turnContext.SendActivityAsync("I didn't understand what you just said to me.");
                            break;
                    }

                }

            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded.Any())
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message.
                        // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != activity.Recipient.Id)
                        {
                            //var welcomeCard = CreateAdaptiveCardAttachment();
                            //var response = CreateResponse(activity, welcomeCard);
                            //await turnContext.SendActivityAsync(response).ConfigureAwait(false);
                        }
                    }
                }
            }

        }


        /// <summary>
        /// Creates a <see cref="HeroCard"/>.
        /// </summary>
        /// <returns>A <see cref="HeroCard"/> the user can view and/or interact with.</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// and <see cref="ActionTypes"/>.</remarks>
        private static HeroCard GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                       " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return heroCard;
        }

        private static HeroCard GetShowDetailsCard()
        {
            String str = SnowQueryTool.getLumbergIncidents();
            var heroCard = new HeroCard
            {
                Title = "Service Now Details Card",
                // Subtitle = "Service Now Details",
                //Text = "these are the details!!!",
                Text = str
                //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                //Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return heroCard;
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        // Load attachment from file.
        private Attachment CreateInitialGreetingCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\initialGreeting.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        // Load attachment from file.
        private Attachment CreateMeetingSummaryCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\meetingSummaryCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        // Load attachment from file.
        private Attachment CreateMeetingsWithBossCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\meetingWithBossCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        
        // Load attachment from file.
        private Attachment CreateEMailSummaryCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\emailSummaryCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        // Load attachment from file.
        private Attachment CreateServiceNowSummaryCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\serviceNowSummaryCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        // Load attachment from file.
        private Attachment CreateServiceNowPendingApprovalsCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\snowPendingApprovalsCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }


        // Load attachment from file.
        private Attachment CreateEMailFromBossCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\emailsFromBossCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        // Load attachment from file.
        private Attachment CreateEncouragementCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Resources\encouragementCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        
    }
}
