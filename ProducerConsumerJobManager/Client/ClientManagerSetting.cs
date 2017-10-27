using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerJobManager.Client
{
    public class ClientManagerSetting
    {
        public string Redis_Server { get; set; }

        public int Redis_DbIndex { get; set; }

        public string ClientList_Hash_Name { get; set; }

        public string ClientList_KeepAlive_Hash_Name { get; set; }
    }
}
