using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Models
{
    public class StartWorkOrderModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("data")]
        public StartWorkOrderData Data { get; set; }

        public class StartWorkOrderData
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}
