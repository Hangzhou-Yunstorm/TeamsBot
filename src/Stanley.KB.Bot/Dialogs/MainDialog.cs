using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Stanley.KB.Bot.Extensions;
using Stanley.KB.Bot.Feedback;
using Stanley.KB.Bot.LanguageGeneration;
using Stanley.KB.Bot.Models;

namespace Stanley.KB.Bot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public static readonly string[] SearchKeyWords = new string[] { "搜索", "查询" };

        private readonly ILogger _logger;
        private readonly FeedbackOptions _feedbackOptions;
        private readonly TemplateManager _templateManager;
        private readonly IStatePropertyAccessor<FeedbackRecord> _feedbackAccessor;
        private readonly IStatePropertyAccessor<List<Activity>> _previousResponseAccessor;
        public MainDialog(IBotTelemetryClient telemetryClient,
            FeedbackOptions feedbackOptions,
            ILogger<MainDialog> logger,
            TemplateManager templateManager,
            UserState userState,
            AzureDialog azureDialog,
            FileSearchDialog fileSearchDialog,
            AdenSolutionDialog adenSolutionDialog)
            : base(nameof(MainDialog))
        {

            _logger = logger;
            _feedbackOptions = feedbackOptions;
            _templateManager = templateManager;
            _previousResponseAccessor = userState.CreateProperty<List<Activity>>(StateProperties.PreviousBotResponse);
            _feedbackAccessor = userState.CreateProperty<FeedbackRecord>(StateProperties.FeedbackRecord);
            TelemetryClient = telemetryClient;

            var steps = new List<WaterfallStep>
            {
                IntroStepAsync,
                RouteStepAsync
            };

            // 启用反馈
            if (_feedbackOptions.FeedbackEnabled)
            {
                steps.Add(ReqeustFeedback);
                steps.Add(RequestFeedbackComment);
                steps.Add(ProcessFeedback);
                AddDialog(new TextPrompt(DialogIds.FeedbackPrompt));
                AddDialog(new TextPrompt(DialogIds.FeedbackCommentPrompt));
            }

            steps.Add(FinalStepAsync);

            AddDialog(new WaterfallDialog(nameof(MainDialog), steps));
            AddDialog(new TextPrompt(DialogIds.NextActionPrompt));
            InitialDialogId = nameof(MainDialog);

            AddDialog(azureDialog);
            AddDialog(fileSearchDialog);
            AddDialog(adenSolutionDialog);
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            // Set up response caching for repeat functionality.
            innerDc.Context.OnSendActivities(StoreOutgoingActivities);
            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            // Set up response caching for repeat functionality.
            innerDc.Context.OnSendActivities(StoreOutgoingActivities);
            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Options is FeedbackUtil.RouteQueryFlag)
            {
                return await stepContext.NextAsync();
            }

            if (stepContext.SuppressCompletionMessage())
            {
                return await stepContext.PromptAsync(DialogIds.NextActionPrompt, new PromptOptions(), cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the defaul if it's first time.
            return await stepContext.PromptAsync(DialogIds.NextActionPrompt, new PromptOptions
            {
                Prompt = stepContext.Options as Activity ?? _templateManager.GenerateActivity("FirstPromptMessage")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> RouteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // 是否是查询文件服务器
            var text = stepContext.Context.Activity.RemoveRecipientMention();

            if (SearchKeyWords.Any(k => text.StartsWith(k)))
            {
                return await stepContext.BeginDialogAsync(nameof(FileSearchDialog), null, cancellationToken);
            }
            //return await stepContext.BeginDialogAsync(nameof(AzureDialog), null, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(AdenSolutionDialog), null, cancellationToken);
        }

        #region 反馈相关
        private async Task<DialogTurnResult> ReqeustFeedback(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(DialogIds.FeedbackPrompt, new PromptOptions
            {
                Prompt = FeedbackUtil.CreateFeedbackActivity(stepContext.Context)
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> RequestFeedbackComment(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Clear feedback state
            await _feedbackAccessor.DeleteAsync(stepContext.Context).ConfigureAwait(false);

            // User dismissed feedback action prompt
            var userResponse = stepContext.Context.Activity.Text;
            if (userResponse == _feedbackOptions.DismissAction.Value)
            {
                return await stepContext.NextAsync();
            }

            var botResponse = await _previousResponseAccessor.GetAsync(stepContext.Context, () => new List<Activity>());
            // Get last activity or previous dialog to send with feedback data
            var feedbackActivity = botResponse.Count >= 2 ? botResponse[^2] : botResponse.LastOrDefault();
            var record = new FeedbackRecord
            {
                Request = feedbackActivity,
                Tag = "EndOfDialogFeedback"
            };

            // User selected a feedback action
            if (_feedbackOptions.FeedbackActions.Any(f => userResponse == f.Value))
            {
                record.Feedback = userResponse;

                await _feedbackAccessor.SetAsync(stepContext.Context, record).ConfigureAwait(false);
                if (_feedbackOptions.CommentsEnabled)
                {
                    return await stepContext.PromptAsync(DialogIds.FeedbackPrompt, new PromptOptions
                    {
                        Prompt = FeedbackUtil.GetFeedbackCommentPrompt(stepContext.Context)
                    }, cancellationToken);
                }
                else
                {
                    return await stepContext.NextAsync();
                }
            }
            else
            {
                return await stepContext.NextAsync(new FeedbackUtil.RouteQueryFlag { RouteQuery = true });
            }
        }

        private async Task<DialogTurnResult> ProcessFeedback(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var record = await _feedbackAccessor.GetAsync(stepContext.Context, () => new FeedbackRecord()).ConfigureAwait(false);
            var passQueryToText = stepContext.Result is FeedbackUtil.RouteQueryFlag;
            var userResponse = stepContext.Context.Activity.Text;

            // Skip this step and pass the query into next step.
            if (passQueryToText)
            {
                return await stepContext.NextAsync(stepContext.Result);
            }
            // User dismissed first feedback prompt, skip this step.
            else if (userResponse == _feedbackOptions.DismissAction.Value &&
               record.Feedback == null)
            {
                return await stepContext.NextAsync();
            }

            if (_feedbackOptions.CommentsEnabled)
            {
                // User responsed to first feedback prompt and replied to comment prompt.
                if (userResponse != _feedbackOptions.DismissAction.Value)
                {
                    record.Comment = userResponse;
                    FeedbackUtil.LogFeedback(record, TelemetryClient);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(_feedbackOptions.CommentReceivedMessage));

                    return await stepContext.NextAsync();
                }
            }

            FeedbackUtil.LogFeedback(record, TelemetryClient);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var passQueryToNext = stepContext.Result is FeedbackUtil.RouteQueryFlag;

            var result = passQueryToNext ? stepContext.Result : _templateManager.GenerateActivity("CompletedMessage");

            return await stepContext.ReplaceDialogAsync(InitialDialogId, result, cancellationToken);
        }

        private async Task<ResourceResponse[]> StoreOutgoingActivities(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var messageActivities = activities.Where(a => a.Type == ActivityTypes.Message).ToList();

            // If the bot is sending message acitivties to user (as opposed to trace activities).
            if (messageActivities.Any())
            {
                var botResponse = await _previousResponseAccessor.GetAsync(turnContext, () => new List<Activity>());

                botResponse = botResponse.Concat(messageActivities)
                    .Where(a => a.ReplyToId == turnContext.Activity.Id)
                    .ToList();

                await _previousResponseAccessor.SetAsync(turnContext, botResponse);
            }
            return await next();
        }
        #endregion

        public static class DialogIds
        {
            public const string FeedbackPrompt = "feedbackPrompt";
            public const string NextActionPrompt = "nextActionPrompt";
            public const string FeedbackCommentPrompt = "feedbackCommentPrompt";
        }
    }
}
