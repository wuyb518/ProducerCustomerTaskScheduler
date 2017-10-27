using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using ProducerConsumerJobManager.Job;
using ProducerConsumerJobManager.Utility;
using StackExchange.Redis;

namespace ProducerConsumerJobManager.JobManager
{
    public class DefaultJobManager<TJob, TId> : IJobManager<TJob, TId> where TJob : IJob<TId>
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultJobManager<TJob, TId>));
        /// <summary>
        /// 配置信息
        /// </summary>
        public JobManagerSetting Setting { get; set; }


        /// <summary>
        /// 任务队列是否为空
        /// </summary>
        public bool IsJobQueueEmpty
        {
            get
            {
                return GetJobCount_InQueue() == 0;
            }
        }


        /// <summary>
        /// 队列中的任务数
        /// </summary>
        /// <returns></returns>
        public long GetJobCount_InQueue()
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);
            var length = db.ListLength(this.Setting.JobList_Queue_Name);
            return length;
        }

        /// <summary>
        /// 总任务数(包含各客户端未完成的)
        /// </summary>
        /// <returns></returns>
        public long GetJobCount_Total()
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);
            var length = db.HashLength(this.Setting.JobList_Hash_Name);
            return length;
        }

        /// <summary>
        /// 任务是否存在
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool IsJobExists(string jobId)
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);
            var exists = db.HashExists(this.Setting.JobList_Hash_Name, jobId);
            return exists;
        }

        /// <summary>
        /// 添加任务到任务队列
        /// </summary>
        /// <param name="job"></param>
        /// <param name="addToQueueLeft">true则添加到队列左侧,false则右侧</param>
        /// <param name="forceAdd">忽略jobidlist_hash是否已经存在,强制添加</param>
        public bool AddJobToQueue(TJob job, bool addToQueueLeft = true, bool forceAdd = false)
        {
            var taskIdStr = job.Id.ToString();
            var taskStr = JsonConvert.SerializeObject(job);
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);
            var tran = db.CreateTransaction();

            //检查任务是否已存在
            var existCondition = Condition.HashNotExists(this.Setting.JobList_Hash_Name, taskIdStr);
            if (!forceAdd)
            {
                //如果不是强制添加,则进行重复判定
                tran.AddCondition(existCondition);
            }
            //强制覆盖hashset
            tran.HashSetAsync(this.Setting.JobList_Hash_Name, taskIdStr, taskStr);
            if (addToQueueLeft)
            {
                //添加到队列左侧
                tran.ListLeftPushAsync(this.Setting.JobList_Queue_Name, taskIdStr);
            }
            else
            {
                //添加到队列右侧
                tran.ListRightPushAsync(this.Setting.JobList_Queue_Name, taskIdStr);
            }

            var result = tran.Execute();
            return result;
        }

        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="job"></param>
        public virtual void EndJob(TJob job)
        {
            if (job == null)
            {
                return;
            }
            var jobId = job.Id.ToString();

            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            db.HashDeleteAsync(this.Setting.JobList_Hash_Name, jobId);
        }

        /// <summary>
        /// 从客户端空间中取出任务(通常是上次客户端没有完成的)
        /// </summary>
        /// <param name="jobSpaceName"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual bool TryGetJob_FromClientJobSpace(string jobSpaceName, out TJob job)
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            var jobIdVal = db.HashGet(jobSpaceName, this.Setting.JobBusiType);

            if (!jobIdVal.HasValue)
            {
                job = default(TJob);
                return false;
            }

            try
            {
                var jobIdStr = jobIdVal.ToString();
                var jobVal = db.HashGet(this.Setting.JobList_Hash_Name, jobIdStr);
                if (!jobVal.HasValue)
                {
                    job = default(TJob);
                    return false;
                }
                try
                {
                    var jobStr = jobVal.ToString();
                    job = JsonConvert.DeserializeObject<TJob>(jobStr);

                    if (job == null)
                    {
                        return false;
                    }

                    return true;
                }
                catch (System.Exception ex)
                {
                    _log.Error($"TryGetJobList_FromClientJobSpace时,解析任务出错, jobSpaceName:{jobSpaceName},jobBusiType:{this.Setting.JobBusiType} jobId:{jobIdStr}, jobVal:{jobVal}", ex);

                    db.HashDelete(this.Setting.JobList_Hash_Name, jobIdStr);
                    throw;
                }
            }
            catch (System.Exception ex)
            {
                _log.Error($"TryGetJobList_FromClientJobSpace时,解析任务出错, jobSpaceName:{jobSpaceName},jobBusiType:{this.Setting.JobBusiType} jobIdVal:{jobIdVal}", ex);

                db.HashDelete(jobSpaceName, this.Setting.JobBusiType);

                job = default(TJob);
                return false;
            }
        }

        public virtual bool TryGetJobList_FromClientJobSpace(string jobSpaceName, out List<TJob> jobList)
        {
            jobList = new List<TJob>();
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            var jobIdVal = db.HashGet(jobSpaceName, this.Setting.JobBusiType);

            if (!jobIdVal.HasValue)
            {
                return false;
            }

            try
            {
                var jobIdList = JsonConvert.DeserializeObject<List<TId>>(jobIdVal.ToString());
                foreach (var jobId in jobIdList)
                {
                    var jobVal = db.HashGet(this.Setting.JobList_Hash_Name, jobId.ToString());
                    if (!jobVal.HasValue)
                    {
                        jobList = new List<TJob>();
                        return false;
                    }
                    try
                    {
                        var jobStr = jobVal.ToString();
                        var job = JsonConvert.DeserializeObject<TJob>(jobStr);

                        if (job == null)
                        {
                            continue;
                        }
                        jobList.Add(job);
                    }
                    catch (System.Exception ex)
                    {
                        _log.Error($"TryGetJobList_FromClientJobSpace时,解析任务出错, jobSpaceName:{jobSpaceName}, jobId:{jobId},jobVal:{jobVal} ", ex);
                        continue;
                    }
                }
            }
            catch (System.Exception ex)
            {
                //解析失败,忽略
                db.HashDelete(jobSpaceName, this.Setting.JobBusiType);
                _log.Error($"TryGetJobList_FromClientJobSpace时,解析任务id列表出错, jobSpaceName:{jobSpaceName}, jobIdVal:{jobIdVal}", ex);
            }

            return jobList.Count > 0;
        }

        /// <summary>
        /// 从客户端空间中取出任务(通常是上次客户端没有完成的)
        /// </summary>
        /// <param name="jobSpaceName"></param>
        /// <returns></returns>
        public void ClearClientJobSpace(string jobSpaceName)
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            db.HashDelete(jobSpaceName, this.Setting.JobBusiType);
        }

        /// <summary>
        /// 从任务队列取出任务
        /// </summary>
        /// <param name="job"></param>
        /// <param name="rightFirst"></param>
        /// <returns></returns>
        public bool TryGetJob_FromJobQueue(out TJob job, bool rightFirst = true)
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            var jobIdVal = rightFirst
                ? db.ListRightPopAsync(this.Setting.JobList_Queue_Name).Result
                : db.ListLeftPopAsync(this.Setting.JobList_Queue_Name).Result;

            if (!jobIdVal.HasValue)
            {
                job = default(TJob);
                return false;
            }

            var jobIdStr = jobIdVal.ToString();
            var jobVal = db.HashGet(this.Setting.JobList_Hash_Name, jobIdStr);
            if (!jobVal.HasValue)
            {
                job = default(TJob);
                return false;
            }
            var jobStr = jobVal.ToString();
            job = JsonConvert.DeserializeObject<TJob>(jobStr);

            if (job == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 重新添加某个客户端任务空间中的任务(通常未完成)到任务队列中
        /// </summary>
        /// <param name="jobSpaceName"></param>
        /// <param name="addToQueueLeft"></param>
        public virtual void ResetJob_FromClientJobSpace(string jobSpaceName, bool addToQueueLeft = false)
        {
            TJob job;
            if (TryGetJob_FromClientJobSpace(jobSpaceName, out job))
            {
                AddJobToQueue(job, addToQueueLeft, true);
            }
        }

        public virtual void ResetJobList_FromClientJobSpace(string jobSpaceName, bool addToQueueLeft = false)
        {
            List<TJob> jobList;
            if (TryGetJobList_FromClientJobSpace(jobSpaceName, out jobList))
            {
                jobList?.ForEach(job =>
                {
                    AddJobToQueue(job, addToQueueLeft, true);
                });

            }
        }

        /// <summary>
        /// 清除所有任务
        /// </summary>
        public virtual void Clear()
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);
            var tran = db.CreateTransaction();
            tran.KeyDeleteAsync(this.Setting.JobList_Queue_Name);
            tran.KeyDeleteAsync(this.Setting.JobList_Hash_Name);

            tran.Execute();
        }

        public virtual TResult GetJobResult<TResult>(TId jobId)
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            var resultVal = db.HashGet(Setting.JobResultList_Hash_Name, jobId.ToString());

            if (!resultVal.HasValue)
            {
                return default(TResult);
            }

            return JsonConvert.DeserializeObject<TResult>(resultVal.ToString());
        }

        public virtual void SaveJobResult<TResult>(TJob job, TResult result)
        {
            var client = RedisWrapper.GetConnection(this.Setting.Redis_Server);
            var db = client.GetDatabase(this.Setting.Redis_DBIndex);

            var resultVal = JsonConvert.SerializeObject(result);
            db.HashSet(Setting.JobResultList_Hash_Name, job.Id.ToString(), resultVal);
        }

    }
}
