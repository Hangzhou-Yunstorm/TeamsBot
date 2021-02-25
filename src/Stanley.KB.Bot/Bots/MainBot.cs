// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Rendering;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stanley.KB.Bot.Dialogs;
using Stanley.KB.Bot.Extensions;
using Stanley.KB.Bot.Models;
using Stanley.KB.Bot.SDP;
using Stanley.KB.Bot.Services;

namespace Stanley.KB.Bot.Bots
{
    public class MainBot : TeamsActivityHandler
    {
        private readonly RequestHelper _request;
        private readonly FileInfosService _fileInfosService;
        private readonly MainDialog _dialog;
        private readonly ConversationState _conversationState;
        private readonly IConfiguration _configuration;
        public MainBot(MainDialog dialog,
            RequestHelper request,
            ConversationState conversationState,
            FileInfosService fileInfosService,
            IConfiguration configuration)
        {
            _dialog = dialog;
            _request = request;
            _configuration = configuration;
            _fileInfosService = fileInfosService;
            _conversationState = conversationState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        /// <summary>
        /// 当用户加入时
        /// </summary>
        /// <param name="membersAdded"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        /// <summary>
        /// 用户点击 “发起请求” 获取 “查看解决方案” 按钮时
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="taskModuleRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            TaskModuleResponse response = null;
            if (taskModuleRequest.Data is JObject @object)
            {
                var taskFetchValue = @object["data"].ToObject<TaskFetchValueModel>();

                // 查看解决方案
                if (taskFetchValue.Type == TaskFetchTypes.Solution)
                {
                    // 显示解决方案
                    response = new TaskModuleResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo()
                            {
                                Title = "查看解决方案",
                                Height = 500,
                                Width = "medium",
                                Url = $"{_configuration["AppHost"]}/solutions/{taskFetchValue.Data}"
                            }
                        }
                    };

                }
                // 发起请求
                else if (taskFetchValue.Type == TaskFetchTypes.AddRequest)
                {
                    var model = JsonConvert.DeserializeObject<InputWorkOrderModel>(taskFetchValue.Data);
                    var card = CreateInputFormAdaptiveCard(model);

                    if (IsRequestCreated(model?.Id))
                    {
                        // TODO 优化
                    }

                    // 显示输入表单
                    response = new TaskModuleResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo()
                            {
                                Title = "发起请求",
                                Height = 500,
                                Width = "medium",
                                Card = new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card }
                            }
                        }
                    };
                }
            }

            if (response == null)
            {
                response = new TaskModuleResponse
                {
                    Task = new TaskModuleMessageResponse
                    {
                        Value = "操作失败"
                    }
                };
            }


            return Task.FromResult(response);
        }

        /// <summary>
        /// 提交请求表单时
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="taskModuleRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            InputWorkOrderModel model = null;
            if (taskModuleRequest.Data is JObject @object)
            {
                model = @object.ToObject<InputWorkOrderModel>();
            }

            if (!string.IsNullOrEmpty(model?.Subject))
            {
                var request = await CreateRequestFromModelAsync(model, turnContext, cancellationToken);
                var result = await _request.AddRequestAsync(request);
                if (result.ResponseStatus.StatusCode == 2000)
                {
                    var reply = MessageFactory.Text($"已为您提交请求[{model.Subject}]({GetRequestUrl(result.Request.Id)})。");
                    var resultCard = CreateResultAdaptiveCardAttachment(result.Request);
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    return new TaskModuleResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo()
                            {
                                Title = "发起请求",
                                Height = 500,
                                Width = "medium",
                                Card = resultCard
                            }
                        }
                    };
                }
                else
                {
                    // TODO 提交失败

                }
            }

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = "数据提交有误",
                },
            };
        }

        private async Task<AddRequestRequestModel> CreateRequestFromModelAsync(InputWorkOrderModel model, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            var requester = member.GetRequesterName();
            return new AddRequestRequestModel
            {
                Request = new AddRequestRequest(model.Subject, requester)
                {
                    Description = model.Description
                }
            };
        }

        /// <summary>
        /// 创建输入表单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private AdaptiveCard CreateInputFormAdaptiveCard(InputWorkOrderModel model, IEnumerable<string> errors = null)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock("Subject"),
                    new AdaptiveTextInput{
                        Id = "subject",
                        IsRequired = true,
                        ErrorMessage = "Subject is required!",
                        Value = model.Subject
                    },
                    new AdaptiveTextBlock("Description"),
                    new AdaptiveTextInput{
                        Id = "description",
                        IsRequired = true,
                        ErrorMessage = "Description is required!",
                        IsMultiline = true,
                        Value = model.Description
                    }
                },
                Actions = new List<AdaptiveAction> {
                    new AdaptiveSubmitAction{ Title = "Submit" }
                }
            };

            return card;
        }

        /// <summary>
        /// 创建提交请求后的结果卡片
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private Attachment CreateResultAdaptiveCardAttachment(SDPRequestCreatedResponse response)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));

            var header = new AdaptiveContainer
            {
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Top,
                                Items = new List<AdaptiveElement>{
                                    new AdaptiveTextBlock(response.Status.Name)
                                    {
                                        Color = AdaptiveTextColor.Accent,
                                        Size = AdaptiveTextSize.Large
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Top,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock($"**#{response.Id} {response.Subject}**")
                                    {
                                        Size = AdaptiveTextSize.Large,
                                        Weight = AdaptiveTextWeight.Bolder
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var content = new AdaptiveContainer
            {
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock(response.Description){
                        Wrap = true,
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveFactSet
                                    {
                                        Facts = new List<AdaptiveFact>
                                        {
                                            new AdaptiveFact("请求类型",response.RequestType.Name),
                                            new AdaptiveFact("模式",response.Mode.Name),
                                            new AdaptiveFact("组",response.Group.Name),
                                            new AdaptiveFact("紧急度",response.Urgency.Name),
                                            new AdaptiveFact("影响",response.Impact.Name),
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveFactSet
                                    {
                                        Facts = new List<AdaptiveFact>
                                        {
                                            new AdaptiveFact("优先级",response.Priority.Name),
                                            new AdaptiveFact("分类",response.Category.Name),
                                            new AdaptiveFact("子分类",response.Subcategory.Name),
                                            new AdaptiveFact("条目",response.Item.Name),
                                        }
                                    }
                                }
                            }
                        }
                    },
                }
            };
            var footer = new AdaptiveContainer
            {
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveActionSet{
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveOpenUrlAction
                            {
                                Title = "打开",
                                Url = new Uri(GetRequestUrl(response.Id))
                            }
                        }
                    }
                }
            };

            card.Body.Add(header);
            card.Body.Add(content);
            card.Body.Add(footer);

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// 判断ID有没有创建过请求
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool IsRequestCreated(string id)
        {
            //TODO
            return false;
        }

        /// <summary>
        /// 获取创建的请求地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetRequestUrl(string id) => $"https://helpme.adenservices.com/WorkOrder.do?woMode=viewWO&woID={id}";
    }
}
