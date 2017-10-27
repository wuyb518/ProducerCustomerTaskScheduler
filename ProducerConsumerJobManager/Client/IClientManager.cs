using System.Collections.Generic;

namespace ProducerConsumerJobManager.Client
{
    public interface IClientManager<TClient> where TClient : ClientInfo
    {
        void Clear();
        void ClientKeepAlive(TClient client);
        List<TClient> GetClients();
        TData GetJobSpaceData<TData>(string jobSpace, string busiType, TData defaultVal = default(TData));
        void RemoveClient(string clientKey);
    }
}