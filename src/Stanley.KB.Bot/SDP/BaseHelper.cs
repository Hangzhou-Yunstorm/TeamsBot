using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.SDP
{
    public abstract class BaseHelper
    {

        protected readonly HttpClient HttpClient;
        public BaseHelper(IConfiguration configuration, HttpClient httpClient)
        {
            HttpClient = httpClient;

            var technicianKey = configuration.GetValue<string>("SDP:ApiKey");
            HttpClient.DefaultRequestHeaders.Add("technician_key", technicianKey);
        }
    }
}
