using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerJobManager.Client
{
    public class ClientManagerConfigurationSection : ConfigurationSection
    {

        [ConfigurationProperty("redis", IsRequired = true)]
        public RedisElement Redis
        {
            get { return (RedisElement)this["redis"]; }
            set { this["redis"] = value; }
        }

        [ConfigurationProperty("setting", IsRequired = true)]
        public ClientManagerSettingElement Setting
        {
            get { return (ClientManagerSettingElement)this["setting"]; }
            set { this["setting"] = value; }
        }

    }

    public class RedisElement : ConfigurationElement
    {
        [ConfigurationProperty("server", DefaultValue = "127.0.0.1:6379", IsRequired = true)]
        public string Server
        {
            get { return (string)this["server"]; }
            set { this["server"] = value; }
        }

        [ConfigurationProperty("dbindex", DefaultValue = 0, IsRequired = true)]
        public int DbIndex
        {
            get { return Convert.ToInt32(this["dbindex"].ToString()); }
            set { this["dbindex"] = value; }
        }
    }

    public class ClientManagerSettingElement : ConfigurationElement
    {
        [ConfigurationProperty("clientlist_hash_name", IsRequired = true)]
        public string ClientList_Hash_Name
        {
            get { return (string)this["clientlist_hash_name"]; }
            set { this["clientlist_hash_name"] = value; }
        }

        [ConfigurationProperty("clientlist_keepalive_hash_name", IsRequired = true)]
        public string ClientList_KeepAlive_Hash_Name
        {
            get { return (string)this["clientlist_keepalive_hash_name"]; }
            set { this["clientlist_keepalive_hash_name"] = value; }
        }
    }
}
