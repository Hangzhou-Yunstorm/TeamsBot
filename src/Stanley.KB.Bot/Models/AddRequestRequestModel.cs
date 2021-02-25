using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Models
{
    public class AddRequestRequestModel
    {
        [JsonProperty("request")]
        public AddRequestRequest Request { get; set; }
    }
    public class AddRequestRequest
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("requester")]
        public Requester Requester { get; set; }
        [JsonProperty("status")]
        public RequestKeyValuePair Status { get; set; }
        [JsonProperty("request_type")]
        public RequestKeyValuePair RequestType { get; set; }
        [JsonProperty("urgency")]
        public RequestKeyValuePair Urgency { get; set; }
        [JsonProperty("impact")]
        public RequestKeyValuePair Impact { get; set; }
        [JsonProperty("category")]
        public RequestKeyValuePair Category { get; set; }
        [JsonProperty("subcategory")]
        public RequestKeyValuePair Subcategory { get; set; }
        [JsonProperty("item")]
        public RequestKeyValuePair Item { get; set; }
        public AddRequestRequest(string subject, string requesterName)
        {
            Subject = subject;
            Status = new RequestKeyValuePair("Open/打开");
            RequestType = new RequestKeyValuePair("Incident");
            Urgency = new RequestKeyValuePair("Low/低");
            Impact = new RequestKeyValuePair("Low/低");
            Category = new RequestKeyValuePair("Operation");
            Subcategory = new RequestKeyValuePair("Other");
            Item = new RequestKeyValuePair("Other");

            Requester = new Requester
            {
                Name = requesterName
            };
        }
    }
    public class Requester
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RequestKeyValuePair
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        public RequestKeyValuePair(string value)
        {
            Name = value;
        }
    }
}
