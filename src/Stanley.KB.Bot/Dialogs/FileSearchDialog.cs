using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Stanley.KB.Bot.Cards;
using Stanley.KB.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Dialogs
{
    public class FileSearchDialog : ComponentDialog
    {
        private readonly IConfiguration _configuration;
        public FileSearchDialog(IBotTelemetryClient telemetryClient,
            IConfiguration configuration)
            : base(nameof(FileSearchDialog))
        {
            _configuration = configuration;
            TelemetryClient = telemetryClient;

            AddDialog(new WaterfallDialog(nameof(FileSearchDialog), new List<WaterfallStep>
            {
                SearchAsync
            }));

            InitialDialogId = nameof(FileSearchDialog);
        }

        private async Task<DialogTurnResult> SearchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var text = stepContext.Context.Activity.RemoveRecipientMention();
            if (!string.IsNullOrEmpty(text))
            {
                var matched = MainDialog.SearchKeyWords.FirstOrDefault(k => text.StartsWith(k));
                var keywords = text.Replace(matched, "").Trim();
                var service = new FileInfosService(_configuration);
                var result = service.SearchAsync(keywords);
                var notEmpty = false;
                var items = new List<Item>();
                await foreach (var item in result)
                {
                    if (!notEmpty)
                    {
                        notEmpty = true;
                    }
                    items.Add(new Item
                    {
                        title = $"{item.name}（{Math.Round(item.size / 1024f, 0)} KB）",
                        subtitle = item.fullname,
                        type = ItemTypes.ResultItem,
                        tap = new CardAction
                        {
                            Type = ActionTypes.OpenUrl,
                            Value = item.fullname
                        }
                    });
                }

                if (notEmpty)
                {
                    var card = new ListCard("为您查找到以下文件", items).ToAttachment();
                    var message = Activity.CreateMessageActivity();
                    message.Attachments.Add(card);
                    await stepContext.Context.SendActivityAsync(message);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"没有找到与“{keywords}”相关的文件。"));
                }
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
