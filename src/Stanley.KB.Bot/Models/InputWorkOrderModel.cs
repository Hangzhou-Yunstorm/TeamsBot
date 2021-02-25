using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Models
{
    public class InputWorkOrderModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
