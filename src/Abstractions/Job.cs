using Abp.Dependency;
using System;

namespace Abp.DelayQueue.Abstractions
{
    /// <summary>
    /// 任务信息
    /// </summary>
    public class Job : IJob
    {
        /// <summary>
        /// 保证全局唯一
        /// </summary>
        public virtual string JobId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 延迟时间
        /// </summary>
        public virtual TimeSpan Delay { get; set; }

        /// <summary>
        /// 默认消息处理时间（5秒）
        /// </summary>
        public virtual TimeSpan TTR { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 数据主体
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public JobStatus Status { get; set; } = JobStatus.READY;
    }
}