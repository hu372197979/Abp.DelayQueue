# Abp.DelayQueue
ABP中实现有赞延迟队列

https://tech.youzan.com/queuing_delay/

# 使用方法详解

### 1、添加ABP依赖
[DependsOn(typeof(DelayQueusModule))]

```csharp
public override void Initialize()
{
    IocManager.RegisterAssembly<FissionApplicationModule>(typeof(Abp.DelayQueue.Abstractions.IJob));
    var thisAssembly = typeof(FissionApplicationModule).GetAssembly();
    IocManager.RegisterAssemblyByConvention(thisAssembly);

    IocManager.UseDealyQueue<FissionApplicationModule>(option =>
    {
        option.BucketDeliveryNumber = 1;
        option.ConsumeReadyNumber = 1;
    });
}
```

### 2、新建一个主题的延迟任务

```csharp
public class ActivityExpired : Job, IJob
{

}
```

### 3、延迟任务处理示例
```csharp
public class ActivityExpiredHandler : IEventHandler<DelayJobEventMessage>, ITransientDependency
{

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<ActivityExpiredHandler> _logger;

    /// <summary>
    /// 处理活动延迟处理
    /// </summary>
    /// <param name="logger"></param>
    public ActivityExpiredHandler(ILogger<ActivityExpiredHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="eventData"></param>
    public void HandleEvent(DelayJobEventMessage eventData)
    {
        var data = eventData.Data as Job;
        _logger.LogInformation(
            $"TestJobHandler消费;Topic:{{Topic}},JobId:{{JobId}},Body:{{Body}}," +
            $"time:{DateTime.Now:yyyy-MM-dd HH:mm:ss}", eventData.Topic, data.JobId, data.Body);
    }
}
```
### 4、发出一个延迟任务

```csharp
public async Task DealyJob()
{
    await iocManager.Resolve<IDelayer>().PutDealyJob(string.Empty, new DelayQueue.ActivityExpired()
    {
        Body = "1111",
        Delay = TimeSpan.FromSeconds(1),
        TTR = TimeSpan.FromSeconds(5)
    });
}
```
