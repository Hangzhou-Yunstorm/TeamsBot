using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Storage
{
    public class RedisStorageSettings
    {
        public string ConnectionString { get; set; }
        public int Database { get; set; }
    }
}
