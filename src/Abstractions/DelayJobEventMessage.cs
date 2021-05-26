using Abp.Events.Bus;

namespace Abp.DelayQueue.Abstractions
{
    /// <summary>
    /// 任务消息
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    public class DelayJobEventMessage : EventData
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="data"></param>
        public DelayJobEventMessage(string topic, object data)
        {
            Data = data;
            Topic = topic;
        }

        /// <summary>
        ///  任务数据
        /// </summary>
        public object Data { get;  }

        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; }
    }
}