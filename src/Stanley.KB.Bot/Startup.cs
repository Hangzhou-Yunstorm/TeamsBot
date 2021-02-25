// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Stanley.KB.Bot.Bots;
using Stanley.KB.Bot.Dialogs;
using Stanley.KB.Bot.Feedback;
using Stanley.KB.Bot.LanguageGeneration;
using Stanley.KB.Bot.SDP;
using Stanley.KB.Bot.Services;
using Stanley.KB.Bot.Storage;
using System;
using System.Collections.Generic;
using System.IO;

namespace Stanley.KB.Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 注册 Hangfire 服务 https://www.hangfire.io/
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseAzureCosmosDbStorage(Configuration["HangFireDb:Url"], Configuration["HangFireDb:AuthSecret"], Configuration["HangFireDb:DatabaseName"], Configuration["HangFireDb:CollectionName"]));
            services.AddHangfireServer();

            // redis 配置信息
            services.Configure<RedisStorageSettings>(Configuration.GetSection("Redis"));

            services.AddControllersWithViews().AddNewtonsoftJson();

            // Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetry();
            // Create the telemetry client.
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
            // Add telemetry initializer that will set the correlation context for all telemetry items.
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            // Add telemetry initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID)
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
            // Create the telemetry middleware to initialize telemetry gathering
            services.AddSingleton<TelemetryInitializerMiddleware>();
            // Create the telemetry middleware(used by the telemetry initializer) to track conversation events
            services.AddSingleton(sp =>
            {
                // 启用个人信息日志记录 用户名和活动文本 等。 https://docs.microsoft.com/zh-cn/azure/bot-service/bot-builder-telemetry?view=azure-bot-service-4.0&tabs=csharp#enable-or-disable-logging-personal-information
                var telemetryClient = sp.GetService<IBotTelemetryClient>();
                return new TelemetryLoggerMiddleware(telemetryClient, logPersonalInformation: true);
            });

            // 反馈设置
            services.AddSingleton(_ => new FeedbackOptions
            {
                FeedbackEnabled = false
            });

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, RedisStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Language Generation https://docs.microsoft.com/zh-cn/azure/bot-service/file-format/bot-builder-lg-file-format?view=azure-bot-service-4.0
            var localizedTemplates = new Dictionary<string, string>();
            localizedTemplates.Add("zh-cn", Path.Combine(".", "Responses", "Responses.lg"));
            services.AddSingleton(new TemplateManager(localizedTemplates, "zh-cn"));

            // The Dialog that will be run by the bot.
            services.AddTransient<MainDialog>()
                .AddTransient<AzureDialog>()
                .AddTransient<FileSearchDialog>();

            // http 服务
            services.AddHttpClient<RequestHelper>();
            services.AddHttpClient<SolutionHelper>();
            services.AddHttpClient<AdenSolutionDialog>(http =>
            {
                http.Timeout = TimeSpan.FromMinutes(2);
            });

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, MainBot>();

            services.AddTransient<FileInfosService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    // 可以通过 /hangfire 访问
                    endpoints.MapHangfireDashboard();
                });

            // app.UseHttpsRedirection();
        }
    }
}
