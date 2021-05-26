using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.DelayQueue
{
    public class DelayQususOption
    {
        /// <summary>
        /// 桶消费数量
        /// </summary>
        public int BucketDeliveryNumber { get; set; }

        /// <summary>
        /// 消费待处理任务数量 默认1个处理者
        /// </summary>
        public int ConsumeReadyNumber { get; set; }
    }
}
