using Stanley.KB.Bot.Services;
using Stanley.KB.Bot.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Stanley.KB.Bot.UnitTests.Services
{
    public class FileInfosServiceTests : BotTestBase
    {


        [Fact]
        public async Task SearchKeyWorkReturnAllMatchedItems()
        {
            var keywork = "ExportTool";
            var service = new FileInfosService(Configuration);
            var result = service.SearchAsync(keywork);

            await foreach (var fileInfo in result)
            {
                Assert.True(fileInfo.name.IndexOf(keywork) >= 0);
            }
        }
    }
}
