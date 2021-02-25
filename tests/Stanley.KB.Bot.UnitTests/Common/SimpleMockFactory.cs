using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Moq;
using Stanley.KB.Bot.LanguageGeneration;
using Stanley.KB.Bot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stanley.KB.Bot.UnitTests.Common
{
    public static class SimpleMockFactory
    {
        public static Mock<TemplateManager> CreateMockTemplateManager(string message)
        {
            var mockTemplateManager = new Mock<TemplateManager>(new Dictionary<string, string>(), "zh-cn");
            mockTemplateManager.Setup(x => x.GenerateActivity(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string templateName, object data) => {
                    return MessageFactory.Text(message);
                });

            return mockTemplateManager;
        }
    }
}
