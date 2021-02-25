using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Stanley.KB.Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Services
{
    public class FileInfosService
    {
        private readonly IConfiguration _configuration;
        public FileInfosService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public async Task<IEnumerable<FileInfo>> SearchAsync(string keyword)
        /// <summary>
        /// 模糊搜索文件名
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<FileInfo> SearchAsync(string keyword, int top = 10)
        {
            var connectionString = _configuration.GetConnectionString("CosmosDb");
            using var client = new CosmosClient(connectionString);
            var database = client.GetDatabase("AdenBot");
            var container = database.GetContainer("FileInfos");

            var sql = $"SELECT * FROM c WHERE CONTAINS(c.name, '{keyword}') OFFSET 0 LIMIT {top}";
            var queryResultSetIterator = container.GetItemQueryIterator<FileInfo>(sql);
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var fileInfo in currentResultSet)
                {
                    yield return fileInfo;
                }
            }
        }
    }
}
