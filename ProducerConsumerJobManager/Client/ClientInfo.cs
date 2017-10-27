using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProducerConsumerJobManager.Client
{
    /// <summary>
    /// 消费者客户端信息
    /// </summary>
    public class ClientInfo
    {
        public ClientSetting Setting { get; set; }

        [JsonIgnore]
        public DateTime? LastKeepAliveAt { get; set; }

        public List<string> BusiTypeList { get; set; }

        /// <summary>
        /// 未完成的任务的存储位置
        /// 用途:
        ///     1.客户端重新启动时,可以优先处理这个任务
        ///     2.客户端offline时,生产者重新将这个任务加入到任务队列
        /// </summary>
        public string JobSpaceName { get; set; }

    }
}
