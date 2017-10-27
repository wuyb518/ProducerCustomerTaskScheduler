using System.Collections.Concurrent;
using StackExchange.Redis;

namespace ProducerConsumerJobManager.Utility
{
    /// <summary>
    /// redis连接池
    /// </summary>
    public static class RedisWrapper
    {

        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> _conn_dic = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        /// <summary>
        /// 从连接池获取
        /// </summary>
        /// <param name="redisServer"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnection(string redisServer)
        {
            ConnectionMultiplexer conn;

            _conn_dic.TryGetValue(redisServer, out conn);

            if (conn == null || !conn.IsConnected)
            {
                conn = ConnectionMultiplexer.Connect(redisServer);

                _conn_dic.AddOrUpdate(redisServer, conn, (k, v) => conn);

            }
            return conn;
        }

    }

}
