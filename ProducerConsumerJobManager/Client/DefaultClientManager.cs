using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using ProducerConsumerJobManager.Utility;

namespace ProducerConsumerJobManager.Client
{
    /// <summary>
    /// 客户端(消费者的)管理器
    /// </summary>
    public class DefaultClientManager<TClient> : IClientManager<TClient> where TClient : ClientInfo
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultClientManager<TClient>));
        public DefaultClientManager(ClientManagerSetting setting)
        {
            this.Setting = setting;
        }

        public ClientManagerSetting Setting { get; set; }


        public List<TClient> GetClients()
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            var clients_hash = db.HashGetAll(this.Setting.ClientList_Hash_Name);

            try
            {
                var clients_dic = clients_hash
                        .Where(t => t.Value.HasValue)
                        .Select(t => JsonConvert.DeserializeObject<TClient>(t.Value))
                        .ToDictionary(t => t.Setting.ClientId, t => t);

                var clients_keepAlive_hash = db.HashGetAll(this.Setting.ClientList_KeepAlive_Hash_Name);

                foreach (var kv in clients_keepAlive_hash)
                {
                    if (!kv.Value.HasValue)
                    {
                        continue;
                    }
                    var key = kv.Name;
                    var value = kv.Value;

                    var clientKey = key;
                    if (clients_dic.ContainsKey(clientKey))
                    {
                        var client = clients_dic[clientKey];
                        DateTime tmpTime;
                        if (DateTime.TryParse(value, out tmpTime))
                        {
                            client.LastKeepAliveAt = tmpTime;
                        }
                        else
                        {
                            //给一个远古时间吧,毕竟null有特殊含义(客户端从未执行keepalive操作)
                            client.LastKeepAliveAt = new DateTime(2000, 1, 1);
                        }
                    }
                }

                return clients_dic.Values.ToList();
            }
            catch (Exception ex)
            {
                _log.Error($"GetClients error, ClientList_Hash_Name:{this.Setting.ClientList_KeepAlive_Hash_Name}", ex);
                db.KeyDelete(this.Setting.ClientList_KeepAlive_Hash_Name);
                return new List<TClient>();
            }

        }

        public TClient GetClient(string clientId)
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            var clientVal = db.HashGet(this.Setting.ClientList_Hash_Name, clientId);

            try
            {
                if (!clientVal.HasValue)
                {
                    return null;
                }
                var client = JsonConvert.DeserializeObject<TClient>(clientVal.ToString());
                if (client == null)
                {
                    return null;
                }

                var client_keepAliveVal = db.HashGet(this.Setting.ClientList_KeepAlive_Hash_Name, clientId);
                if (client_keepAliveVal.HasValue)
                {
                    DateTime tmpTime;
                    if (DateTime.TryParse(client_keepAliveVal.ToString(), out tmpTime))
                    {
                        client.LastKeepAliveAt = tmpTime;
                    }
                    else
                    {
                        //给一个远古时间吧,毕竟null有特殊含义(客户端从未执行keepalive操作)
                        client.LastKeepAliveAt = new DateTime(2000, 1, 1);
                    }
                }

                return client;
            }
            catch (Exception ex)
            {
                _log.Error($"GetClient error, ClientList_Hash_Name:{this.Setting.ClientList_KeepAlive_Hash_Name},clientId:{clientId}", ex);
                db.HashDelete(this.Setting.ClientList_KeepAlive_Hash_Name, clientId);
                return null;
            }
        }

        /// <summary>
        /// 移除指定客户端
        /// </summary>
        /// <param name="clientKey">客户端key</param>
        public void RemoveClient(string clientKey)
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            var client = GetClient(clientKey);
            if (client != null)
            {
                ClearJobSpace(client.JobSpaceName);
            }
            db.HashDelete(this.Setting.ClientList_Hash_Name, clientKey);
            db.HashDelete(this.Setting.ClientList_KeepAlive_Hash_Name, clientKey);
        }

        public void Clear()
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            var clients = GetClients();
            foreach (var client in clients)
            {
                ClearJobSpace(client.JobSpaceName);
            }

            db.KeyDelete(this.Setting.ClientList_Hash_Name);
            db.KeyDelete(this.Setting.ClientList_KeepAlive_Hash_Name);
        }

        /// <summary>
        /// 客户端 keepalive, todo:看起来放到client中更合适一些
        /// </summary>
        /// <param name="client"></param>
        public void ClientKeepAlive(TClient client)
        {
            var clientIdStr = client.Setting.ClientId;
            var clientStr = JsonConvert.SerializeObject(client);
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            db.HashSet(this.Setting.ClientList_Hash_Name, clientIdStr, clientStr);
            db.HashSet(this.Setting.ClientList_KeepAlive_Hash_Name, clientIdStr, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public void SetJobSpaceData<TData>(string jobSpace, string busiType, TData data)
        {
            if (typeof(TData).IsPrimitive)
            {
                //todo 基元类型特殊处理
            }
            var dataStr = JsonConvert.SerializeObject(data);

            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            db.HashSet(jobSpace, busiType, dataStr);
        }

        public TData GetJobSpaceData<TData>(string jobSpace, string busiType, TData defaultVal = default(TData))
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            var dataVal = db.HashGet(jobSpace, busiType);
            if (!dataVal.HasValue)
            {
                return defaultVal;
            }

            try
            {
                if (typeof(TData).IsPrimitive)
                {
                    //todo 基元类型特殊处理
                }
                var data = JsonConvert.DeserializeObject<TData>(dataVal.ToString());

                return data;
            }
            catch (Exception ex)
            {
                db.HashDelete(jobSpace, busiType);
                _log.Error($"GetJobSpaceData,解析数据出错, jobSpace:{jobSpace},busiType:{busiType} ", ex);
                return defaultVal;
            }

        }

        /// <summary>
        /// jobSpace下的数据全部删除
        /// </summary>
        /// <param name="jobSpace"></param>
        public void ClearJobSpace(string jobSpace)
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            db.KeyDelete(jobSpace);
        }

        /// <summary>
        /// jobSpace下对应busiType的数据删除
        /// </summary>
        /// <param name="jobSpace"></param>
        /// <param name="busiType"></param>
        public void ClearJobSpace(string jobSpace, string busiType)
        {
            var conn = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = conn.GetDatabase(this.Setting.Redis_DbIndex);

            db.HashDelete(jobSpace, jobSpace);
        }

    }
}
