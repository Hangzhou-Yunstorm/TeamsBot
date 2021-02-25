using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.SDP
{
    public class SolutionHelper : BaseHelper
    {
        private readonly ILogger<SolutionHelper> _logger;
        public SolutionHelper(IConfiguration configuration, HttpClient httpClient, ILogger<SolutionHelper> logger) : base(configuration, httpClient)
        {
            _logger = logger;
        }

        public async Task<SolutionResponse> GetSolutionAsync(string id)
        {
            try
            {
                var url = $"https://helpme.adenservices.com/api/v3/solutions/{id}";
                var response = await HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SolutionResponse>(json);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Fetch solution {id} error: {e.Message}");
            }

            return default;
        }
    }
}
