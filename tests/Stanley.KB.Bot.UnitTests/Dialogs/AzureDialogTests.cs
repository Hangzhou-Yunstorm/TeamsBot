using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Stanley.KB.Bot.Dialogs;
using Stanley.KB.Bot.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Stanley.KB.Bot.UnitTests.Dialogs
{
    public class AzureDialogTests : BotTestBase
    {
        private readonly IBotTelemetryClient _telemetryClient;
        public AzureDialogTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _telemetryClient = new NullBotTelemetryClient();
        }

        [Fact]
        public async Task ShowsAnswerFromQnAMaker()
        {
            var mockTemplateManager = SimpleMockFactory.CreateMockTemplateManager("mock template message");
            var azureDialog = new AzureDialog(_configurationLazy.Value, _telemetryClient, mockTemplateManager.Object);
            var testClient = new DialogTestClient(Channels.Test, azureDialog, middlewares: new[] { new XUnitDialogTestLogger(Output) });

            var reply = await testClient.SendActivityAsync<IMessageActivity>("azure database for mysql");

            Assert.NotNull(reply);
        }
    }
}
