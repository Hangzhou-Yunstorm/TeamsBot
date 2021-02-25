using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace Stanley.KB.Bot.UnitTests.Common
{
    /// <summary>
    /// A base class with helper methods and properties to write bot tests.
    /// </summary>
    public abstract class BotTestBase
    {
        // A lazy configuration object that gets instantiated once during execution when is needed
        protected static readonly Lazy<IConfiguration> _configurationLazy = new Lazy<IConfiguration>(() =>
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            return config.Build();
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="BotTestBase"/> class.
        /// </summary>
        protected BotTestBase()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotTestBase"/> class.
        /// </summary>
        /// <param name="output">
        /// An XUnit <see cref="ITestOutputHelper"/> instance.
        /// See <see href="https://xunit.net/docs/capturing-output.html">Capturing Output</see> in the XUnit documentation for additional details.
        /// </param>
        protected BotTestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        public virtual IConfiguration Configuration => _configurationLazy.Value;

        protected ITestOutputHelper Output { get; }

    }
}
