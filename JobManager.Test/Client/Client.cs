using System.Collections.Generic;
using ProducerConsumerJobManager.Client;

namespace JobManager.Test.Client
{
    /// <summary>
    /// 客户端 单例模式
    /// </summary>
    public class Client : ClientInfo
    {
        private Client()
        {
        }

        static Client()
        {
            var setting = new ClientSetting
            {
                ClientId = "testclient",
                ClientIP = "127.0.0.1",
                ClientName = "测试客户端"
            };

            var jobSpaceName = $"{setting.ClientId}_jobspace";
            var busiTypeList = new List<string> { ClientConfig.JobBusiType };

            Instance = new Client
            {
                Setting = setting,
                JobSpaceName = jobSpaceName,
                BusiTypeList = busiTypeList
            };
        }

        public static Client Instance { get; }

    }
}
