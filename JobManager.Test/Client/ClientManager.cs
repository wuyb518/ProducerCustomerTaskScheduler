using System;
using System.Threading;
using System.Threading.Tasks;
using ProducerConsumerJobManager.Client;
using JobManager.Test.TestJob;

namespace JobManager.Test.Client
{
    public class ClientManager : DefaultClientManager<Client>
    {
        private static ClientManager _instance;

        public static TimeSpan KeepAliveInterval { get; set; } = new TimeSpan(0, 0, 10);
        public static TimeSpan OfflineInterval { get; set; } = new TimeSpan(0, 30, 0);

        private bool _isStandKeepAlive = false;

        public ClientManager(ClientManagerSetting setting) : base(setting)
        {
        }

        public static ClientManager GetInstance()
        {
            if (_instance == null)
            {
                var clientManagerSetting = new ClientManagerSetting
                {
                    Redis_Server = Test_Config.Redis_Server,
                    Redis_DbIndex = Test_Config.Redis_DBIndex,
                    ClientList_Hash_Name = ClientConfig.ClientList_Hash_Name,
                    ClientList_KeepAlive_Hash_Name = ClientConfig.ClientList_KeepAlive_Hash_Name
                };
                var clientManager = new ClientManager(clientManagerSetting);

                Interlocked.CompareExchange(ref _instance, clientManager, null);
            }
            return _instance;
        }


        public void StandKeepAliveAsnyc(CancellationToken token)
        {
            if (_isStandKeepAlive)
            {
                return;
            }
            _isStandKeepAlive = true;
            var task = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        ClientKeepAlive(Client.Instance);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }

                    var nextTime = DateTime.Now.Add(KeepAliveInterval);
                    SpinWait.SpinUntil(() => DateTime.Now > nextTime);
                }

                _isStandKeepAlive = false;
            }, token);
        }
    }
}
