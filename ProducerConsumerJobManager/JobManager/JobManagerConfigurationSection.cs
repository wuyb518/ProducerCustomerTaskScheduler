using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProducerConsumerJobManager.Client;

namespace ProducerConsumerJobManager.JobManager
{

    public class JobManagerConfigurationSectionGroup : ConfigurationSectionGroup
    {

    }

    public class JobManagerConfigurationSection : ConfigurationSection
    {

        [ConfigurationProperty("redis", IsRequired = true)]
        public RedisElement Redis
        {
            get { return (RedisElement)this["redis"]; }
            set { this["redis"] = value; }
        }

        [ConfigurationProperty("setting", IsRequired = true)]
        public JobSettingElement Setting
        {
            get { return (JobSettingElement)this["setting"]; }
            set { this["setting"] = value; }
        }

    }

    public class JobSettingElement : ConfigurationElement
    {
        [ConfigurationProperty("joblist_queue_name", IsRequired = true)]
        public string JobList_Queue_Name
        {
            get { return (string)this["joblist_queue_name"]; }
            set { this["joblist_queue_name"] = value; }
        }

        [ConfigurationProperty("joblist_hash_name", IsRequired = true)]
        public string JobList_Hash_Name
        {
            get { return (string)this["joblist_hash_name"]; }
            set { this["joblist_hash_name"] = value; }
        }

        [ConfigurationProperty("jobresultlist_hash_name", IsRequired = true)]
        public string JobResultList_Hash_Name
        {
            get { return (string)this["jobresultlist_hash_name"]; }
            set { this["jobresultlist_hash_name"] = value; }
        }

        [ConfigurationProperty("job_busitype", IsRequired = true)]
        public string JobBusiType
        {
            get { return (string)this["job_busitype"]; }
            set { this["job_busitype"] = value; }
        }

        [ConfigurationProperty("job_customer_process_count", IsRequired = true)]
        public string Job_Customer_Process_Count
        {
            get { return (string)this["job_customer_process_count"]; }
            set { this["job_customer_process_count"] = value; }
        }

        [ConfigurationProperty("job_customer_success_count", IsRequired = true)]
        public string Job_Customer_Success_Count
        {
            get { return (string)this["job_customer_success_count"]; }
            set { this["job_customer_success_count"] = value; }
        }
    }
}
