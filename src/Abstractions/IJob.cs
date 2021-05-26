using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.DelayQueue.Abstractions
{
    /// <summary>
    /// 任务接口
    /// </summary>
    public interface IJob : ITransientDependency
    {
        /// <summary>
        /// 保证全局唯一
        /// </summary>
        string JobId { get; set; }

        /// <summary>
        /// 延迟时间
        /// </summary>
        TimeSpan Delay { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        TimeSpan TTR { get; set; }

        /// <summary>
        /// 数据主体
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        JobStatus Status { get; set; }

    }

    /// <summary>
    /// 任务状态
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// 可执行状态，等待消费
        /// </summary>
        READY,
        /// <summary>
        /// 不可执行状态，等待时钟周期
        /// </summary>
        DELAY,
        /// <summary>
        /// 已被消费者读取，但还未得到消费者的响应
        /// </summary>
        RESERVED,
        /// <summary>
        /// 已被消费完成或者已被删除
        /// </summary>
        DELETED
    }
}
