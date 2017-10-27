using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerJobManager.JobManager
{
    public class JobManagerSetting
    {
        public string Redis_Server { get; set; }
        public int Redis_DBIndex { get; set; }

        public string JobList_Queue_Name { get; set; }
        public string JobList_Hash_Name { get; set; }
        public string JobResultList_Hash_Name { get; set; }

        public string JobBusiType { get; set; }
    }
}
