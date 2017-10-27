using System.Threading;
using ProducerConsumerJobManager.JobManager;

namespace JobManager.Test.TestJob
{
    public class Test_JobManager : DefaultJobManager<Job, int>
    {
        private static Test_JobManager _instance;

        public static Test_JobManager GetInstance()
        {
            if (_instance == null)
            {
                var jobManagerSetting = new JobManagerSetting
                {
                    Redis_Server = Test_Config.Redis_Server,
                    Redis_DBIndex = Test_Config.Redis_DBIndex,
                    JobList_Queue_Name = Test_Config.JobList_Queue_Name,
                    JobList_Hash_Name = Test_Config.JobList_Hash_Name,
                    JobResultList_Hash_Name = Test_Config.JobResultList_Hash_Name,
                    JobBusiType = Test_Config.JobBusiType
                };
                var jobManager = new Test_JobManager
                {
                    Setting = jobManagerSetting
                };

                Interlocked.CompareExchange(ref _instance, jobManager, null);
            }
            return _instance;
        }

    }
}
