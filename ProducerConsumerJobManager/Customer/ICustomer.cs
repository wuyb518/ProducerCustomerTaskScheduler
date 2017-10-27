using ProducerConsumerJobManager.Job;


namespace ProducerConsumerJobManager.Customer
{
    public interface ICustomer<TJob, TId> where TJob : IJob<TId>
    {
    }
}
