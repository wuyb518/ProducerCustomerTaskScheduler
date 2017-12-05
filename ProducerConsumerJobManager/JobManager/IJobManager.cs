using System.Collections.Generic;
using ProducerConsumerJobManager.Job;

namespace ProducerConsumerJobManager.JobManager
{
    public interface IJobManager<TJob, in TId> where TJob : class, IJob<TId>
    {
        bool IsJobQueueEmpty { get; }
        JobManagerSetting Setting { get; }

        bool AddJobToQueue(TJob job, bool upperScore = true, bool allowCover = false);
        void Clear();
        void ClearClientJobSpace(string jobSpaceName);
        void EndJob(TJob job);
        long GetJobCount_InQueue();
        long GetJobCount_Total();
        bool IsJobExists(TId jobId);
        TJob GetJob(TId jobId);
        void ResetJob_FromClientJobSpace(string jobSpaceName);
        void ResetJobList_FromClientJobSpace(string jobSpaceName);
        void SaveJobResult<TResult>(TJob job, TResult result);
        bool TryGetJobList_FromClientJobSpace(string jobSpaceName, out List<TJob> jobList);
        bool TryGetJob_FromClientJobSpace(string jobSpaceName, out TJob job);
        bool TryGetJob_FromJobQueue(out TJob job);
    }
}