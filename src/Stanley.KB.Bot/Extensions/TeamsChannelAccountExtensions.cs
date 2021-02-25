using Microsoft.Bot.Schema.Teams;
using System.Linq;

namespace Stanley.KB.Bot.Extensions
{
    public static class TeamsChannelAccountExtensions
    {
        /// <summary>
        /// 从 TeamsChannelAccount 中获取请求者名称
        /// </summary>
        /// <param name="account"></param>
        /// <param name="defaultName"></param>
        /// <returns></returns>
        public static string GetRequesterName(this TeamsChannelAccount account, string defaultName = "AGISVC-ITSMBOT")
        { 
            return account != null ? account.GivenName + " " + account.Surname : defaultName;
        }
    }
}
