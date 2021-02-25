using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Stanley.KB.Bot.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Stanley.KB.Bot.Models.StartWorkOrderModel;

namespace Stanley.KB.Bot.Dialogs
{
    /// <summary>
    /// 搜索 从 Azure Docs 生成的知识库，未使用
    /// </summary>
    public class AzureDialog : ComponentDialog
    {
        public const string DefaultCardTitle = "你是想找：";
        public const string DefaultCardNoMatchText = "以上都不是";

        public AzureDialog(IConfiguration configuration,
                IBotTelemetryClient telemetryClient,
                 TemplateManager templateManager) : base(nameof(AzureDialog))
        {
            AddDialog(new AzureQnAMakerDialog(configuration, telemetryClient, templateManager));
            var steps = new List<WaterfallStep>
            {
                InitialStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(AzureDialog), steps));

            InitialDialogId = nameof(AzureDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);
        }

        public class AzureQnAMakerDialog : QnAMakerDialog
        {
            private readonly IConfiguration _configuration;
            private readonly TemplateManager _templateManager;
            public AzureQnAMakerDialog(IConfiguration configuration,
                IBotTelemetryClient telemetryClient,
                 TemplateManager templateManager) : base()
            {
                TelemetryClient = telemetryClient;
                _configuration = configuration;
                _templateManager = templateManager;
            }

            protected override async Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
            {
                var endpoint = new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _configuration["QnAMaker:KnowledgebaseId"],
                    EndpointKey = _configuration["QnAMaker:EndpointKey"],
                    Host = _configuration["QnAMaker:EndpointHostName"]
                };
                return new QnAMaker(endpoint, null, null, TelemetryClient);
            }

            protected override Task<QnAMakerOptions> GetQnAMakerOptionsAsync(DialogContext dc)
            {
                return Task.FromResult(new QnAMakerOptions
                {
                    ScoreThreshold = _configuration.GetValue("QnAMaker:ScoreThreshold", 0.3f),
                    Top = _configuration.GetValue("QnAMaker:Top", 5),
                    QnAId = 0,
                    IsTest = false,
                    RankerType = "Default"
                });
            }

            protected override Task<QnADialogResponseOptions> GetQnAResponseOptionsAsync(DialogContext dc)
            {
                return Task.FromResult(new QnADialogResponseOptions
                {
                    ActiveLearningCardTitle = DefaultCardTitle,
                    CardNoMatchText = DefaultCardNoMatchText,
                    CardNoMatchResponse = _templateManager.GenerateActivity("CardNoMatch"),
                    NoAnswer = GenerateNoAnswerActivity(dc),
                });
            }

            private Activity GenerateNoAnswerActivity(DialogContext dc)
            {
                var activity = dc.Context.Activity.CreateReply("没有找到匹配的答案。");
                var card = new ThumbnailCard();

                // TODO 使用 TaskFetchValueModel
                var data = new StartWorkOrderData
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Text = dc.Context.Activity.RemoveRecipientMention()
                };
                card.Text = "您可以选择发起请求，获得更多帮助：";
                card.Buttons = new List<CardAction>();
                card.Buttons.Add(new CardAction("invoke", "发起请求", value: new { type = "task/fetch", data }));

                activity.Attachments = new List<Attachment> { card.ToAttachment() };

                return activity;
            }
        }
    }
}
