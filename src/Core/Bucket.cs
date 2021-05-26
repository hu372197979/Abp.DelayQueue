using Abp.Runtime.Caching.Redis;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using System.Linq;
using Abp.Dependency;

namespace Abp.DelayQueue.Core
{
    /// <summary>
    /// 桶
    /// </summary>
    public class Bucket: ITransientDependency
    {
        /// <summary>
        /// 桶前缀
        /// </summary>
        private const string queuePrefix = "delay-queue-bucket:";

        /// <summary>
        /// REDIS操作对象
        /// </summary>
        private readonly IDatabase _database;

        /// <summary>
        /// 桶构造
        /// </summary>
        /// <param name="redisCacheDatabaseProvider"></param>
        public Bucket(IAbpRedisCacheDatabaseProvider redisCacheDatabaseProvider)
        {
            _database = redisCacheDatabaseProvider.GetDatabase();
        }


        /// <summary>
        /// 推送消息进桶
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="jobId">任务编号</param>
        /// <param name="delay">延迟时间</param>
        /// <returns>处理结果</returns>
        public Task<bool> PushJobToBucketAsync(string topic, string jobId, TimeSpan delay)
        {
            var delaySec = GetDelaySeconds(delay);
            return _database.SortedSetAddAsync($"{queuePrefix}{topic}", jobId, Convert.ToDouble(delaySec));
        }


        /// <summary>
        /// 得到的时间戳
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private long GetDelaySeconds(TimeSpan delay)
        {
            return new DateTimeOffset(DateTime.Now.Add(delay).ToUniversalTime()).ToUnixTimeSeconds();
        }


        /// <summary>
        /// 获取已到过期时间的任务
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="limit">数量</param>
        /// <returns>得到的待处理的任务</returns>
        public Task<RedisValue[]> GetExpireJobsAsync(string topic, long limit)
        {
           return _database.SortedSetRangeByScoreAsync($"{queuePrefix}{topic}", 0.0f, GetDelaySeconds(TimeSpan.Zero), take: limit);
        }


        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Task<bool> RemoveJobAsync(string topic, string jobId)
        {
            return _database.SortedSetRemoveAsync($"{queuePrefix}{topic}", jobId);
        }

        /// <summary>
        /// 获取下一个任务执行时间
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task<DateTime?> GetNextJobExecTimeAsync(string topic)
        {
            var items = await _database.SortedSetRangeByScoreWithScoresAsync($"{queuePrefix}{topic}", 0.0f, double.MaxValue, take: 1);
            if (items == null || items.Length == 0)
                return null;
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(items[0].Score.ToString("0000"))).LocalDateTime;
        }

    }
}