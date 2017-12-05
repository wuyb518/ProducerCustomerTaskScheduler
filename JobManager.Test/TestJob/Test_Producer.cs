using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ProducerConsumerJobManager.Producer;
using JobManager.Test.Client;
using ProducerConsumerJobManager.Utility;

namespace JobManager.Test.TestJob
{
    public class Test_Producer : IProducer<Job, int>
    {

        private static readonly TimeSpan _resetJobInterval = new TimeSpan(0, 0, 30);


        public void Process(CancellationToken token)
        {
            var resetJobTokenSource = new CancellationTokenSource();
            StandResetOfflineClientJobsAsync(_resetJobInterval, resetJobTokenSource.Token);

            var random = new Random();

            var timestamp = DateTimeHelper.GetCurrentLongTimestamp();
            for (var i = 0; i < 10000; i++)
            {
                var delay = random.Next(70, 200);
                var job = new Job
                {
                    Id = i,
                    Score = timestamp
                };
                Test_JobManager.GetInstance().AddJobToQueue(job);

                Console.WriteLine($"添加, {job.Id}");
                Thread.Sleep(delay);
            }
        }

        public Task StandResetOfflineClientJobsAsync(TimeSpan interval, CancellationToken token)
        {
            //Task.Factory.StartNew传入cancellationToken应该可以自动阻断代码,所以内部写while(true)也可以
            var task = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        this.ResetOfflineClientJobs(interval);
                    }
                    catch (Exception ex)
                    {
                    }

                    var nextTime = DateTime.Now.Add(interval);
                    SpinWait.SpinUntil(() => DateTime.Now > nextTime);
                }
            }, token);

            return task;
        }


        public void ResetOfflineClientJobs(TimeSpan interval)
        {
            var clients = ClientManager.GetInstance().GetClients();

            var clients_toRemove = new List<Client.Client>();
            foreach (var client in clients)
            {
                if (client.LastKeepAliveAt != null && client.LastKeepAliveAt < DateTime.Now.Add(-interval))
                {
                    //移除客户端
                    clients_toRemove.Add(client);
                }
            }

            foreach (var client in clients_toRemove)
            {
                //重置任务
                Test_JobManager.GetInstance().ResetJob_FromClientJobSpace(client.JobSpaceName);

                //删除客户端,不在这里删了
                //ClientManager.RemoveClient(client.Setting.ClientId);
            }
        }
    }
}
