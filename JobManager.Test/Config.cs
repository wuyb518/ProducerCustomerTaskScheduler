using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobManager.Test
{
    public class Config
    {

        public static string RedisServer { get; set; }
        public static int RedisDBIndex { get; set; }
               
        public static string JobList_Queue_Name { get; set; }
        public static string JobList_Hash_Name { get; set; }
        public static string JobResultList_Hash_Name { get; set; }
             

        public static string ClientList_Hash_Name { get; set; }
        public static string ClientList_KeepAlive_Hash_Name { get; set; }
               

        public static string JobBusiType { get; set; }
    }
}
