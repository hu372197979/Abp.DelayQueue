using System.Threading.Tasks;
using Abp.DelayQueue.Abstractions;
using Abp.Dependency;
using Abp.Runtime.Caching.Redis;
using StackExchange.Redis;

namespace Abp.DelayQueue.Core
{

    /// <summary>
    /// 任务池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JobPool: ITransientDependency
    {
        /// <summary>
        /// 任务池前缀
        /// </summary>
        private const string _prefix = "delay-queue-jobpool:";

        /// <summary>
        /// Redis操作对象
        /// </summary>
        private readonly IDatabase _database;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="redisCacheDatabaseProvider"></param>
        public JobPool(IAbpRedisCacheDatabaseProvider redisCacheDatabaseProvider)
        {
            _database = redisCacheDatabaseProvider.GetDatabase();
        }

        /// <summary>
        /// 将任务添加进任务池
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public Task<bool> PutJobAsync<T>(T job) where T: Job
        {
            //检验jobid是否全局唯一？
            return _database.StringSetAsync($"{_prefix}{job.JobId}", Newtonsoft.Json.JsonConvert.SerializeObject(job));
        }

        /// <summary>
        /// 获取指定任务数据
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Task<T> GetJobAsync<T>(string jobId)
        {
            string ret = _database.StringGet($"{_prefix}{jobId}");
            if (string.IsNullOrWhiteSpace(ret))
            {
                return Task.FromResult(default(T));
            }
            return Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(ret));
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Task<bool> DelJobAsync(string jobId)
        {
            return _database.KeyDeleteAsync($"{_prefix}{jobId}");
        }

    }
}