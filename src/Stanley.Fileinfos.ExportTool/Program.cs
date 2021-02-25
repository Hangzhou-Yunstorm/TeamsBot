using CommandLine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.ConsoleColor;
using Konsole.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using Konsole;
using System.Text;

namespace Stanley.Fileinfos.ExportTool
{
    class Program
    {
        public const string Version = "1.0.0";

        private static ConcurrentQueue<ExportFileInfo> Files = new ConcurrentQueue<ExportFileInfo>();
        private static Options Options;
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine($"======== Aden Bot FileInfo Export Tool (Ver.{Version})========");
            Console.WriteLine();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // get options
            var result = Parser.Default.ParseArguments<Options>(args);
            try
            {
                Options = result?.MapResult(options => options, null);
                if (Options != null)
                {
                    Console.WriteLine($"Getting files under {Options.RootPath}");
                    GetDirectories(Options.RootPath);
                    await ImportToCosmosDb(configuration);
                }
            }
            catch (NullReferenceException e)
            {
            }
        }

        private static void GetAllFiles(string path)
        {
            FileInfo[] files = null;
            try
            {
                var root = new DirectoryInfo(path);
                files = root.GetFiles();

            }
            catch (UnauthorizedAccessException e)
            {
            }
            if (files != null)
            {
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 10 }, file =>
                {
                    var fileInfo = ExtraFileInfo(file);
                    Files.Enqueue(fileInfo);
                });
            }
        }

        private static void GetDirectories(string path)
        {
            DirectoryInfo[] directories = null;
            GetAllFiles(path);
            try
            {
                var root = new DirectoryInfo(path);
                directories = root.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {
            }

            if (directories != null)
            {
                Parallel.ForEach(directories, new ParallelOptions { MaxDegreeOfParallelism = 10 }, dir => GetDirectories(dir.FullName));
            }
        }

        private static ExportFileInfo ExtraFileInfo(FileInfo fileInfo)
        {
            var dir = new DirectoryInfo(Options.RootPath);
            var fullName = fileInfo.FullName.Replace(dir.FullName, "");
            if (fullName.StartsWith("\\"))
            {
                fullName = $"{Options.Origin}{fullName}";
            }
            else
            {
                fullName = $"{Options.Origin}//{fullName}";
            }
            return new ExportFileInfo
            {
                id = fullName,
                name = fileInfo.Name,
                fullname = fullName,
                extension = fileInfo.Extension,
                size = fileInfo.Length
            };
        }

        private static async Task ImportToCosmosDb(IConfiguration configuration)
        {
            var total = Files.Count;
            var failures = 0;
            var endpointUrl = configuration["CosmosDb:EndpointUrl"];
            var authorizationKey = configuration["CosmosDb:AuthorizationKey"];
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:ContainerName"];
            var client = new CosmosClient(endpointUrl, authorizationKey, new CosmosClientOptions() { AllowBulkExecution = true });
            Database database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            var containerProperties = new ContainerProperties(containerName, "/fullname");
            var throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(5000);
            Container container = await database.CreateContainerIfNotExistsAsync(containerProperties, throughputProperties);

            Console.WriteLine($"Got {total} files, processing...");
            var itemsToInsert = new Dictionary<PartitionKey, Stream>(total);

            while (!Files.IsEmpty && Files.TryDequeue(out var file))
            {
                var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, file);
                itemsToInsert.Add(new PartitionKey(file.fullname), stream);
            }

            Console.WriteLine("Importing to database, please wait patiently...");
            var stopwatch = Stopwatch.StartNew();
            List<Task> tasks = new List<Task>(total);
            foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
            {
                tasks.Add(CreateOrUpdateAsync(container, item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using ResponseMessage response = task.Result;
                            if (!response.IsSuccessStatusCode)
                            {
                                Interlocked.Increment(ref failures);
                                Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                            }
                        }));
            }
            //Interlocked.Increment();
            // Wait until all are done
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine($"Import data completed：");
            Console.WriteLine($"- Total：{total}");
            Console.WriteLine($"- Succeed：{total - failures}");
            Console.WriteLine($"- Failed：{failures}");
            Console.WriteLine($"- Elapsed：{stopwatch.Elapsed}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }

        private static Task<ResponseMessage> CreateOrUpdateAsync(Container container, Stream value, PartitionKey key)
        {
            return container.UpsertItemStreamAsync(value, key);
            //return container.CreateItemStreamAsync(value, key);
        }
    }
}
