using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using Stanley.KB.Bot.Dialogs;
using Stanley.KB.Bot.Feedback;
using Stanley.KB.Bot.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Stanley.KB.Bot.UnitTests.Common;
using Xunit.Abstractions;
using Microsoft.Bot.Schema;

namespace Stanley.KB.Bot.UnitTests.Dialogs
{
    public class MainDialogTests : BotTestBase
    {
        private readonly IBotTelemetryClient _telemetryClient;
        private readonly UserState _userState;
        private readonly Mock<ILogger<MainDialog>> _mockLogger;
        public MainDialogTests(ITestOutputHelper output) : base(output)
        {
            _mockLogger = new Mock<ILogger<MainDialog>>();
            _telemetryClient = new NullBotTelemetryClient();
            _userState = new UserState(new MemoryStorage());
        }

        [Fact]
        public async Task ShowsFirstMessageAndCallsAzureDialogDirectly()
        {
            // Arrange
            var feedbackOptions = new FeedbackOptions();
            var mockTemplateManager = SimpleMockFactory.CreateMockTemplateManager("mock template message");

            var mockDialog = new Mock<AzureDialog>(Configuration, _telemetryClient, mockTemplateManager.Object);
            mockDialog.Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(async (DialogContext dialogContext, object options, CancellationToken cancellationToken) =>
                {
                    dialogContext.Dialogs.Add(new TextPrompt("MockDialog"));
                    return await dialogContext.PromptAsync("MockDialog",
                        new PromptOptions() { Prompt = MessageFactory.Text($"{nameof(AzureDialog)} mock invoked") }, cancellationToken);
                });
            var fileSearchDialog = new Mock<FileSearchDialog>(_telemetryClient, Configuration);

            var mainDialog = new MainDialog(_telemetryClient, feedbackOptions, _mockLogger.Object, mockTemplateManager.Object, _userState, mockDialog.Object, fileSearchDialog.Object);
            var testClient = new DialogTestClient(Channels.Test, mainDialog, middlewares: new[] { new XUnitDialogTestLogger(Output) });

            // Act/Assert
            var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");
            Assert.Equal("mock template message", reply.Text);

            reply = await testClient.SendActivityAsync<IMessageActivity>("next");
            Assert.Equal($"{nameof(AzureDialog)} mock invoked", reply.Text);
        }
    }
}
