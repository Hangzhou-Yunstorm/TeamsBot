using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Feedback
{
    public class FeedbackUtil
    {
        /// <summary>
        /// Create a prompt for feedback activity with message text and feedback actions defined by passed FeedbackOptions parameter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="feedbackOptions"></param>
        /// <returns></returns>
        public static Activity CreateFeedbackActivity(ITurnContext context, FeedbackOptions feedbackOptions = null)
        {
            feedbackOptions ??= new FeedbackOptions();
            var choices = new List<Choice>(feedbackOptions.FeedbackActions)
            {
                feedbackOptions.DismissAction
            };
            var feedbackActivity = ChoiceFactory.ForChannel(context.Activity.ChannelId, choices, feedbackOptions.FeedbackPromptMessage);
            return feedbackActivity as Activity;
        }

        /// <summary>
        /// Create a prompt for feedback comment activity with message text and dismiss action defined by passed FeedbackOptions parameter.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Activity GetFeedbackCommentPrompt(ITurnContext context, FeedbackOptions feedbackOptions = null)
        {
            feedbackOptions ??= new FeedbackOptions();
            var choices = new List<Choice> {
                feedbackOptions.DismissAction
            };
            var message = ChoiceFactory.ForChannel(context.Activity.ChannelId, choices, $"{feedbackOptions.FeedbackReceivedMessage}，{feedbackOptions.CommentPrompt}");

            return message as Activity;
        }

        public static void LogFeedback(FeedbackRecord record, IBotTelemetryClient telemetryClient)
        {
            var properties = new Dictionary<string, string>
            {
                {nameof(FeedbackRecord.Tag), record.Tag },
                {nameof(FeedbackRecord.Feedback), record.Feedback },
                {nameof(FeedbackRecord.Comment), record.Comment },
                {nameof(FeedbackRecord.Request.Id), record.Request.Id },
                {nameof(FeedbackRecord.Request.Text), record.Request.Text },
                {nameof(FeedbackRecord.Request.ChannelId), record.Request.ChannelId },
            };
            telemetryClient.TrackEvent("Feedback", properties);
        }

        internal class RouteQueryFlag
        {
            public bool RouteQuery { get; set; }
        }
    }
}
