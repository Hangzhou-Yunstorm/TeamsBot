// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Stanley.KB.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Threading.ThreadPool.SetMinThreads(100, 100);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, logger) =>
                {
                    logger.WriteTo.Console()
                        .WriteTo.RollingFile("logs\\{Date}.log", shared: true, restrictedToMinimumLevel: LogEventLevel.Warning);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
