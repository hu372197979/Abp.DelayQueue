using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Abp.DelayQueue.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Abp.DelayQueue.Core;
using Abp.Dependency;
using Abp.Events.Bus;

namespace Abp.DelayQueue
{

    /// <summary>
    /// 延迟消息处理接口
    /// </summary>
    public interface IDelayedMessageProcessor
    {
        /// <summary>
        /// 投递消息进待处理消息队列处理
        /// </summary>
        /// <returns></returns>
        Task DeliveryToReadyQueue(string topic);

        /// <summary>
        /// 消费待处理队列
        /// </summary>
        /// <returns></returns>
        Task ConsumeReadyJob(string topic);
    }


    /// <summary>
    /// 延迟消息处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelayedMessageProcessor : IDelayedMessageProcessor
    {

        private readonly ILogger<DelayedMessageProcessor> _logger;
        private readonly DelayQususOption _delayQususOption;

        /// <summary>
        /// 延迟消息处理
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        public DelayedMessageProcessor(
            DelayQususOption option,
            ILogger<DelayedMessageProcessor> logger)
        {
            _logger = logger;
            _delayQususOption = option;
        }

        /// <summary>
        /// 递送消息到待处理队列
        /// </summary>
        /// <returns></returns>
        public async Task DeliveryToReadyQueue(string topic)
        {
            var jobPool = IocManager.Instance.Resolve<JobPool>();
            var bucket = IocManager.Instance.Resolve<Bucket>();
            var readyQueue = IocManager.Instance.Resolve<ReadyQueue>();
            while (true)
            {
                try
                {
                    var jobids = await bucket.GetExpireJobsAsync(topic, 10);
                    if (jobids == null || jobids.Length == 0)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    foreach (var jobid in jobids)
                    {
                        //获取任务数据
                        var job = await jobPool.GetJobAsync<Job>(jobid);
                        if (job != null)
                        {
                            if (job.Status == JobStatus.RESERVED)
                            {
                                //当前任务在消费处理中 （添加防止处理超时的任务）
                                _logger.LogInformation($"增加延迟任务触发:{Newtonsoft.Json.JsonConvert.SerializeObject(job)}");
                                // 任务等待处理超时
                                job.Status = JobStatus.DELAY;
                                // 修改任务池状态
                                await jobPool.PutJobAsync(job);
                                // 删除当前任务
                                await bucket.RemoveJobAsync(topic, jobid);
                                // 添加任务超时任务（防止任务处理失败可新触发消费）
                                await bucket.PushJobToBucketAsync(topic, jobid, job.TTR);
                            }
                            else if (job.Status == JobStatus.DELAY)
                            {
                                _logger.LogInformation($"延迟处理时间到达触发:{Newtonsoft.Json.JsonConvert.SerializeObject(job)}");
                                // 延时任务
                                job.Status = JobStatus.READY;
                                // 修改任务池状态
                                await jobPool.PutJobAsync(job);
                                // 设置到待处理任务
                                await readyQueue.PushToReadyQueue(topic, job);
                                // 删除当前任务
                                await bucket.RemoveJobAsync(topic, jobid);
                            }
                            else if (job.Status == JobStatus.DELETED)
                            {
                                _logger.LogInformation($"任务完成删除任务触发:{Newtonsoft.Json.JsonConvert.SerializeObject(job)}");
                                // 桶中的任务删除
                                await bucket.RemoveJobAsync(topic, jobid);
                                // 删除任务池中的当前任务
                                await jobPool.DelJobAsync(jobid);
                            }
                            else
                            {
                                _logger.LogInformation($"开始处理待处理的任务触发:{Newtonsoft.Json.JsonConvert.SerializeObject(job)}");
                                // 设置到待处理任务
                                await readyQueue.PushToReadyQueue(topic, job);
                                // 删除当前任务
                                await bucket.RemoveJobAsync(topic, jobid);
                            }
                        }
                        else
                        {
                            await bucket.RemoveJobAsync(topic, jobid);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"DeliveryToReadyQueue Error;Topic:{topic}");
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// 待处理任务消费处理
        /// </summary>
        /// <returns></returns>
        public async Task ConsumeReadyJob(string topic)
        {
            var jobPool = IocManager.Instance.Resolve<JobPool>();
            var readyQueue = IocManager.Instance.Resolve<ReadyQueue>();
            var bucket = IocManager.Instance.Resolve<Bucket>();
            while (true)
            {
                try
                {
                    //这里取出任务数据 存在一定风险 POP出来 断电 ~~~ 玩完~~ 
                    var job = await readyQueue.GetJobFromReadyQueue<Job>(topic);
                    if (job == null)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    #region 修改任务为消费处理中状态
                    //获取元任务信息
                    job = await jobPool.GetJobAsync<Job>(job.JobId);
                    //取到任务执行任务处理
                    if (job.Status == JobStatus.READY)
                    {
                        job.Status = JobStatus.RESERVED;
                        // 修改任务池状态
                        await jobPool.PutJobAsync(job);
                        // 修改完状态立即添加一个桶处理
                        await IocManager.Instance.Resolve<Bucket>().PushJobToBucketAsync(topic, job.JobId, TimeSpan.Zero);
                    } 
                    #endregion

                    var message = new DelayJobEventMessage(topic, job);
                    EventBus.Default.Trigger(message);

                    #region 修改任务为删除状态
                    //获取元任务信息
                    job = await jobPool.GetJobAsync<Job>(job.JobId);
                    //取到任务执行任务处理
                    if (job.Status == JobStatus.RESERVED)
                    {
                        job.Status = JobStatus.DELETED;
                        // 修改任务池状态
                        await jobPool.PutJobAsync(job);
                        // 修改完任务状态触发处理
                        await IocManager.Instance.Resolve<Bucket>().PushJobToBucketAsync(topic, job.JobId, TimeSpan.Zero);
                    } 
                    #endregion

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"ConsumeReadyJob Error; Topic:{topic}");
                    await Task.Delay(1000);
                }
            }
        }
    }
}