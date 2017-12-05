using ProducerConsumerJobManager.Job;

namespace JobManager.Test.TestJob
{
    public class Job : IJob<int>
    {
        public int Id { get; set; }

        public double Score { get; set; }

    }
}
