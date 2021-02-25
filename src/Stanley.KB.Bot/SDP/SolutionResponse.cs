using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.SDP
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class CreatedTime
    {
        public string display_value { get; set; }
        public string value { get; set; }
    }

    public class ApprovalStatus
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class CreatedBy
    {
        public string email_id { get; set; }
        public string name { get; set; }
        public bool is_vipuser { get; set; }
        public string id { get; set; }
    }

    public class LastUpdatedBy
    {
        public object email_id { get; set; }
        public string name { get; set; }
        public bool is_vipuser { get; set; }
        public string id { get; set; }
    }

    public class LastUpdatedTime
    {
        public string display_value { get; set; }
        public string value { get; set; }
    }

    public class Topic
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Solution
    {
        public CreatedTime created_time { get; set; }
        public bool has_attachments { get; set; }
        public ApprovalStatus approval_status { get; set; }
        public bool review_notified { get; set; }
        public object expiry_date { get; set; }
        public string description { get; set; }
        public List<string> key_words { get; set; }
        public string title { get; set; }
        public object solution_owner { get; set; }
        public CreatedBy created_by { get; set; }
        public bool expiry_notified { get; set; }
        public LastUpdatedBy last_updated_by { get; set; }
        public object review_date { get; set; }
        public bool @public { get; set; }
        public LastUpdatedTime last_updated_time { get; set; }
        public Topic topic { get; set; }
        public string comment { get; set; }
        public string id { get; set; }
        public string view_count { get; set; }
        public object user_groups { get; set; }
    }

    public class Message
    {
        public string status_code { get; set; }
        public string type { get; set; }
        public string message { get; set; }
    }

    public class ResponseStatus
    {
        public List<Message> messages { get; set; }
        public string status { get; set; }
    }

    public class SolutionResponse
    {
        public Solution solution { get; set; }
        public ResponseStatus response_status { get; set; }
    }


}
