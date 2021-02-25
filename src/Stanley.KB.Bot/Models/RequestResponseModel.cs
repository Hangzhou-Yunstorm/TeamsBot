using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Models
{
    /// <summary>
    /// 创建请求的响应
    /// </summary>
    public class RequestResponseModel : SDPResponseBase
    {
        public SDPRequestCreatedResponse Request { get; set; }
    }

    /// <summary>
    /// 创建成功后的响应模型
    /// </summary>
    public class SDPRequestCreatedResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("created_time")]
        public SDPCreatedTimeResponse CreatedTime { get; set; }
        [JsonProperty("status")]
        public SDPIdNameAndColorResponse Status { get; set; }
        [JsonProperty("request_type")]
        public SDPIdAndNameResponse RequestType { get; set; }
        [JsonProperty("category")]
        public SDPIdAndNameResponse Category { get; set; }
        [JsonProperty("subcategory")]
        public SDPIdAndNameResponse Subcategory { get; set; }
        [JsonProperty("item")]
        public SDPIdAndNameResponse Item { get; set; }
        [JsonProperty("urgency")]
        public SDPIdAndNameResponse Urgency { get; set; }
        [JsonProperty("impact")]
        public SDPIdAndNameResponse Impact { get; set; }
        [JsonProperty("requester")]
        public SDPRequesterResponse Requester { get; set; }
        [JsonProperty("group")]
        public SDPGroupResponse Group { get; set; }
        [JsonProperty("mode")]
        public SDPIdAndNameResponse Mode { get; set; }
        [JsonProperty("sla")]
        public SDPIdAndNameResponse SLA { get; set; }
        [JsonProperty("priority")]
        public SDPIdNameAndColorResponse Priority { get; set; }
        [JsonProperty("created_by")]
        public SDPCreatedByResponse CreatedBy { get; set; }
    }

    public class SDPCreatedByResponse : SDPRequesterResponse
    {
        [JsonProperty("department")]
        public SDPGroupResponse Department { get; set; }
    }

    public class SDPGroupResponse : SDPIdAndNameResponse
    {
        [JsonProperty("site")]
        public SDPIdAndNameResponse Site { get; set; }
    }


    public class SDPRequesterResponse : SDPIdAndNameResponse
    {
        [JsonProperty("email_id")]
        public string EmailId { get; set; }
        [JsonProperty("is_vipuser")]
        public bool IsVipUser { get; set; }
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public class SDPCreatedTimeResponse
    {
        [JsonProperty("display_value")]
        public string DisplayValue { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class SDPIdAndNameResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class SDPIdNameAndColorResponse : SDPIdAndNameResponse
    {
        [JsonProperty("color")]
        public string Color { get; set; }
    }


    public class SDPResponseBase
    {
        [JsonProperty("response_status")]
        public SDPResponseStatus ResponseStatus { get; set; }
    }

    /// <summary>
    /// 状态
    /// </summary>
    public class SDPResponseStatus
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("messages")]
        public IList<SDPResponseMessage> Messages { get; set; }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public class SDPResponseMessage
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
