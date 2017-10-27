using ProducerConsumerJobManager.Job;

namespace ProducerConsumerJobManager.Producer
{
    public interface IProducer<out TJob, TId> where TJob : IJob<TId>
    {
     
    }
}