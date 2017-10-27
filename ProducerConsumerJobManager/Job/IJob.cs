using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerJobManager.Job
{
    public interface IJob<TId>
    {
        TId Id { get; set; }
    }
}
