using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobManager.Test.Client
{
    public static class ClientConfig
    {
        public static string ClientList_Hash_Name { get; set; } = "customer_clientlist";
        public static string ClientList_KeepAlive_Hash_Name { get; set; } = "customer_clientlist_keepalive";

        public static string JobBusiType = "test";
    }
}
