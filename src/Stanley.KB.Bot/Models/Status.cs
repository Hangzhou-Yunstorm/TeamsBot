using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Models
{
    public enum Status
    {
        /// <summary>
        /// 打开
        /// </summary>
        Open = 1,
        /// <summary>
        /// 搁置
        /// </summary>
        Onhold = 2,
        /// <summary>
        /// 关闭
        /// </summary>
        Closed = 3,
        /// <summary>
        /// 已解决
        /// </summary>
        Resolved = 4,
        /// <summary>
        /// 指派中
        /// </summary>
        Assigned = 5,
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress = 6,
        /// <summary>
        /// 等待用户响应
        /// </summary>
        WaitingForUserInformation = 301,
        /// <summary>
        /// 等待变更请求
        /// </summary>
        WaitingForChangeRequest = 302,
    }
}
