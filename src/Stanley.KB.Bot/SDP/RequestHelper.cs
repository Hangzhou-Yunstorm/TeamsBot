using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Stanley.KB.Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.SDP
{
    public class RequestHelper : BaseHelper
    {
        public RequestHelper(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient)
        {
        }

        /// <summary>
        /// 添加请求
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RequestResponseModel> AddRequestAsync(AddRequestRequestModel model)
        {
            var url = "https://helpme.adenservices.com/api/v3/requests";

            var inputData = new Dictionary<string, string>
            {
                { "input_data",  JsonConvert.SerializeObject(model)}
            };
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(inputData)
            };
            var response = await HttpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RequestResponseModel>(body);

            return result;
        }

        /// <summary>
        /// 关闭请求
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public async Task<SDPResponseBase> CloseRequestAsync(string requestId)
        {
            var resolution = "<div></div>";
            var url = $"https://helpme.adenservices.com/api/v3/requests/{requestId}/close";
            var inputData = new Dictionary<string, string>
            {
                { "input_data",  $"{{\"request\":{{\"status\":{{\"id\":\"3\"}}, \"closure_info\":{{\"requester_ack_resolution\":false,\"requester_ack_comments\":null,\"closure_comments\":\"AdenBot 自动关闭在 {DateTime.UtcNow:yyyy-MM-dd hh:mm:ss} UTC.\",\"closure_code\":null}},\"is_fcr\":false,\"resolution\":{{\"content\":\"{resolution}\"}}}}"}
            };
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new FormUrlEncodedContent(inputData)
            };
            var response = await HttpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SDPResponseBase>(body);
            // TODO 提醒已自动关闭请求
            return result;
        }

        /// <summary>
        /// 将请求的状态设置为已解决
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public Task<RequestResponseModel> ResolvedRequestAsync(string requestId)
        {
            return UpdateRequestStatus(requestId, Status.Resolved);
        }


        /// <summary>
        /// 更新请求状态
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="status">Resolved/已解决</param>
        /// <returns></returns>
        public async Task<RequestResponseModel> UpdateRequestStatus(string requestId, Status status)
        {
            var url = $"https://helpme.adenservices.com/api/v3/requests/{requestId}";
            var data = new
            {
                request = new
                {
                    resolution = new
                    {
                        content = $"AdenBot 自动关闭在 {DateTime.UtcNow:yyyy-MM-dd hh:mm:ss} UTC."
                    },
                    status = new
                    {
                        id = ((int)status).ToString()
                    }
                }
            };
            var inputData = new Dictionary<string, string>
            {
                { "input_data", JsonConvert.SerializeObject(data)}
            };
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new FormUrlEncodedContent(inputData)
            };
            var response = await HttpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RequestResponseModel>(body);
        }
    }
}
