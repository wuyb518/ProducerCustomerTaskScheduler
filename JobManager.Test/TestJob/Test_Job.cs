using ProducerConsumerJobManager.Job;
using System;

namespace JobManager.Test.TestJob
{
    public class Job : IJob<int>
    {
        public int Id { get; set; }
    }
}
