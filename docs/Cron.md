## Cron

CronUtility，内置调度任务中使用

> 使用示例

```js
/// <summary>
///     任务计数
/// </summary>
public void Increment(DateTimeOffset dateTimeOffset)
{
    TryCount++;
    IsRunning = false;
    LastRunTime = NextRunTime;
    if (IsSingle)
    {
        NextRunTime = null;
    }
    else if (FixedDelay > 0)
    {
        NextRunTime = dateTimeOffset.AddSeconds(FixedDelay);
    }
    else if (!string.IsNullOrWhiteSpace(CronExpress))
    {
        NextRunTime = CronUtility.GetNextOccurrence(CronExpress, dateTimeOffset);
    }
}
```
