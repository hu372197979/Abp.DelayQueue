using System;
using System.Collections.Concurrent;
using System.Threading;
using Abp.DelayQueue.Abstractions;
using Abp.Dependency;
using Abp.Runtime.Caching.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Abp.Modules;
using Castle.MicroKernel.Registration;

namespace Abp.DelayQueue
{
    public static class DelayQususExtensions
    {

        /// <summary>
        /// 延迟队列频道信息
        /// </summary>
        private static readonly ConcurrentBag<string> DelayQueues = new ConcurrentBag<string>();

        /// <summary>
        /// 添加延迟任务
        /// </summary>
        /// <param name="cachingConfiguration"></param>
        public static void UseDealyQueue<TModule>(this IIocManager iocManager, Action<DelayQususOption> option) where TModule : AbpModule
        {
            iocManager.Register<DelayQususOption>(DependencyLifeStyle.Singleton);
            iocManager.RegisterIfNot(typeof(IDelayedMessageProcessor), typeof(DelayedMessageProcessor), DependencyLifeStyle.Singleton);
            iocManager.RegisterIfNot(typeof(IDelayer), typeof(DefaultDelayer), DependencyLifeStyle.Singleton);
            option.Invoke(iocManager.Resolve<DelayQususOption>());
            var jobs = iocManager.ResolveAll<IJob>();
            //当前所有任务执行处理
            jobs.ToList().ForEach(_job =>
            {
                RegisterDealyQueueJob(iocManager, _job);
            });
        }

        /// <summary>
        /// 开始执行延迟任务处理
        /// </summary>
        /// <param name="iocManager">IOC容器</param>
        /// <param name="job">任务</param>
        private static void RegisterDealyQueueJob(IIocManager iocManager, IJob job)
        {
            var topic = job.GetType().Name;
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException($"异常的Topic");
            }
            //配置信息(增加 桶池 以及 )
            var _option = iocManager.Resolve<DelayQususOption>();
            //避免重复注册
            if (!DelayQueues.Contains(topic))
            {
                DelayQueues.Add(topic);
                var processor = iocManager.Resolve<IDelayedMessageProcessor>();
                for (int i = 0; i < _option.BucketDeliveryNumber; i++)
                {
                    new Thread(async () => await processor.DeliveryToReadyQueue(topic)).Start();
                }
                for (int i = 0; i < _option.ConsumeReadyNumber; i++)
                {
                    new Thread(async () => await processor.ConsumeReadyJob(topic)).Start();
                }
            }
        }

    }
}