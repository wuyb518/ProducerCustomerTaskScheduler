using System;
using System.Threading;
using System.Threading.Tasks;


namespace JobManager.Test
{
    class Program
    {

        static void Main(string[] args)
        {

            Client.ClientManager.GetInstance().StandKeepAliveAsnyc(CancellationToken.None);
            TestJob.Test_JobManager.GetInstance().Clear();
            ActProducer();
            //ActCustomer();

            Console.ReadLine();
        }




        public static void ActProducer()
        {
            Task.Factory.StartNew(() =>
            {
                var producer = new TestJob.Test_Producer();
                producer.Process(CancellationToken.None);
            });

        }

        public static void ActCustomer()
        {
            Task.Factory.StartNew(() =>
            {
                var customer = new TestJob.Test_Customer();
                customer.Process(CancellationToken.None);
            });

        }
    }
}
