using System;
using System.Threading;
using ProducerConsumerJobManager.Customer;
using ProducerConsumerJobManager.Utility;

namespace JobManager.Test.TestJob
{
    public class Test_Customer : ICustomer<Job, int>
    {
        /// <summary>
        /// 公众号画像统计_消费者
        /// </summary>
        public void Process(CancellationToken token)
        {
            Job job;
            if (Test_JobManager.GetInstance().TryGetJob_FromClientJobSpace(Client.Client.Instance.JobSpaceName, out job))
            {
                ProcessJob(job);
            }

            while (!token.IsCancellationRequested)
            {
                if (Test_JobManager.GetInstance().TryGetJob_FromJobQueue(out job))
                {
                    ProcessJob(job);
                }
                else
                {
                    Console.WriteLine("没有任务了...\r\n\r\n\r\n");
                    Thread.Sleep(3000);
                    //break;
                }

            }
            //全部处理完成

           

        }


        public void ProcessJob(Job job)
        {

            var random = new Random();
            var delay = random.Next(300, 1000);
            if (random.Next(1, 100) < 80)
            {
                Console.WriteLine($"完成, {job.Id}");
                FinishJob(job);
                //清空任务空间
                Test_JobManager.GetInstance().ClearClientJobSpace(Client.Client.Instance.JobSpaceName);

            }
            else
            {
                Console.WriteLine($"失败, {job.Id}");
                EndJob(job);
                //清空任务空间
                Test_JobManager.GetInstance().ClearClientJobSpace(Client.Client.Instance.JobSpaceName);
            }
            Thread.Sleep(delay);
        }


        public void EndJob(Job job)
        {
            Test_JobManager.GetInstance().EndJob(job);
            var connection = RedisWrapper.GetConnection(Test_Config.Redis_Server);
            var db = connection.GetDatabase(Test_Config.Redis_DBIndex);

            //处理数量+1
            db.StringIncrement(Test_Config.Job_Customer_Process_Count);
           
        }

        public void FinishJob(Job job)
        {
            Test_JobManager.GetInstance().EndJob(job);
            var connection = RedisWrapper.GetConnection(Test_Config.Redis_Server);
            var db = connection.GetDatabase(Test_Config.Redis_DBIndex);

            //处理数量+1
            db.StringIncrement(Test_Config.Job_Customer_Process_Count);
            //成功数量+1
            db.StringIncrement(Test_Config.Job_Customer_Success_Count);

        }
    }
}
