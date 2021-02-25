using Hangfire;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Stanley.KB.Bot.Extensions;
using Stanley.KB.Bot.Models;
using Stanley.KB.Bot.SDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Dialogs
{
    /// <summary>
    /// 解决方案知识库
    /// </summary>
    public class AdenSolutionDialog : ComponentDialog
    {
        private readonly QnAMaker _qna;
        private readonly RequestHelper _request;
        public AdenSolutionDialog(IConfiguration configuration, IBotTelemetryClient telemetryClient, HttpClient http, RequestHelper request) : base(nameof(AdenSolutionDialog))
        {
            _request = request;
            TelemetryClient = telemetryClient;
            var endpoint = new QnAMakerEndpoint()
            {
                Host = configuration["AdenSolutionsQnAMaker:EndpointHostName"],
                EndpointKey = configuration["AdenSolutionsQnAMaker:EndpointKey"],
                KnowledgeBaseId = configuration["AdenSolutionsQnAMaker:KnowledgebaseId"]
            };
            var options = new QnAMakerOptions
            {
                Top = configuration.GetValue("AdenSolutionsQnAMaker:Top", 5),
                ScoreThreshold = configuration.GetValue("AdenSolutionsQnAMaker:ScoreThreshold", 0.3f)
            };
            _qna = new QnAMaker(endpoint, options, http, telemetryClient);

            var steps = new List<WaterfallStep>
            {
                FindSolutionAsync,
                NoAnswerAsync
            };

            AddDialog(new WaterfallDialog(nameof(AdenSolutionDialog), steps));

            InitialDialogId = nameof(AdenSolutionDialog);
        }

        /// <summary>
        /// 查找解决方案
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FindSolutionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // 创建请求时的标题
            var subject = $"From Aden Test Bot：{stepContext.Context.Activity.RemoveRecipientMention().Replace("\r", "").Replace("\n", "")}";
            // 获取答案
            var results = await _qna.GetAnswersAsync(stepContext.Context);
            // 匹配到解决方案
            if (results.Any())
            {
                // QnA Maker 答案中存放的是解决方案 ID
                var answer = results.FirstOrDefault();
                var data = new TaskFetchValueModel
                {
                    Data = answer.Answer,
                    Type = TaskFetchTypes.Solution
                };
                var card = new ThumbnailCard
                {
                    Buttons = new List<CardAction>{
                        new CardAction("invoke", "查看解决方案", value: new { type = "task/fetch", data })
                    }
                };
                var activity = stepContext.Context.Activity.CreateReply("为您找到以下解决方案：");
                activity.Attachments.Add(card.ToAttachment());
                // 发送找到解决方案的消息，包含“查看解决方案”按钮
                await stepContext.Context.SendActivityAsync(activity);

                // 并自动发起请求
                var member = await TeamsInfo.GetMemberAsync(stepContext.Context, stepContext.Context.Activity.From.Id, cancellationToken);
                var requester = member.GetRequesterName();
                var request = new AddRequestRequestModel
                {
                    Request = new AddRequestRequest(subject, requester)
                };
                var result = await _request.AddRequestAsync(request);
                if (result.ResponseStatus.StatusCode == 2000)
                {
                    // TODO 记录自动创建的请求，后续可对这个请求追踪状态变化
                    var reply = MessageFactory.Text($"已自动创建请求【[{subject}](https://helpme.adenservices.com/WorkOrder.do?woMode=viewWO&woID={result.Request.Id})】。");
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                    // 15 分钟后自动修改这个请求的状态
                    BackgroundJob.Schedule<RequestHelper>(r => r.ResolvedRequestAsync(result.Request.Id), TimeSpan.FromMinutes(15));
                }

                // 结束对话
                return await stepContext.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
            }
            return await stepContext.NextAsync(subject, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 未匹配到解决方案则可以发起请求
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> NoAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var subject = stepContext.Result as string;
            var activity = stepContext.Context.Activity.CreateReply("没有找到匹配的答案。");
            var card = new ThumbnailCard();

            // 选择发起工单
            var data = new TaskFetchValueModel
            {
                Type = TaskFetchTypes.AddRequest,
                Data = JsonConvert.SerializeObject(new InputWorkOrderModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Subject = subject
                })
            };
            card.Text = "您可以选择发起请求，获得更多帮助：";
            card.Buttons = new List<CardAction>();
            card.Buttons.Add(new CardAction("invoke", "发起请求", value: new { type = "task/fetch", data }));
            activity.Attachments = new List<Attachment> { card.ToAttachment() };
            await stepContext.Context.SendActivityAsync(activity, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
        }
    }
}
