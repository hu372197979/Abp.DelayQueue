using System;
using System.Threading.Tasks;
using Abp.DelayQueue.Abstractions;
using Abp.Dependency;
using Abp.Runtime.Caching.Redis;
using StackExchange.Redis;

namespace Abp.DelayQueue.Core
{

    /// <summary>
    /// 准备处理任务队列
    /// </summary>
    public class ReadyQueue: ITransientDependency
    {
        /// <summary>
        /// 准备处理队列
        /// </summary>
        private const string queuePrefix = "delay-ready-queue:";

        /// <summary>
        /// REDIS操作对象
        /// </summary>
        private readonly IDatabase _database;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="redisCacheDatabaseProvider"></param>
        public ReadyQueue(IAbpRedisCacheDatabaseProvider redisCacheDatabaseProvider)
        {
            _database = redisCacheDatabaseProvider.GetDatabase();
        }

        /// <summary>
        /// 压入一个待处理的任务
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public Task PushToReadyQueue<T>(string topic, T job) where T: Job
        {
            return _database.ListRightPushAsync($"{queuePrefix}{topic}", Newtonsoft.Json.JsonConvert.SerializeObject(job));
        }

        /// <summary>
        /// 弹出一个任务处理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public Task<T> GetJobFromReadyQueue<T>(string topic)
        {
            string data = _database.ListLeftPop($"{queuePrefix}{topic}");
            if (string.IsNullOrEmpty(data))
            {
                return Task.FromResult(default(T));
            }
            return Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data));
        }

    }
}