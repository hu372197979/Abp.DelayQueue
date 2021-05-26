using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Abp.DelayQueue.Abstractions;
using Abp.DelayQueue.Core;
using Abp.Dependency;

namespace Abp.DelayQueue
{

    /// <summary>
    /// 延迟任务管理器
    /// </summary>
    public class DefaultDelayer : IDelayer
    {

        /// <summary>
        /// 添加一个延迟任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task PutDealyJob(string topic, Job job)
        {
            topic = string.IsNullOrWhiteSpace(topic) ? job.GetType().Name: topic;
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException($"异常的Topic");
            }
            await IocManager.Instance.Resolve<JobPool>().PutJobAsync(job);
            await IocManager.Instance.Resolve<Bucket>().PushJobToBucketAsync(topic, job.JobId, job.Delay);
        }

        /// <summary>
        /// 删除一个延迟任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public async Task RemoveDealyJob(string topic, string jobId)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException($"异常的Topic");
            }
            if (await IocManager.Instance.Resolve<JobPool>().DelJobAsync(jobId))
            {
                await IocManager.Instance.Resolve<Bucket>().RemoveJobAsync(topic, jobId);
            }
        }

    }
}