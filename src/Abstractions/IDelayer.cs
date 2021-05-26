using System;
using System.Threading.Tasks;

namespace Abp.DelayQueue.Abstractions
{
    /// <summary>
    /// 延迟任务
    /// </summary>
    public interface IDelayer
    {
        /// <summary>
        /// 添加延迟任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task PutDealyJob(string topic, Job job);
        
        /// <summary>
        /// 删除延迟任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task RemoveDealyJob(string topic, string jobId);
    }
}