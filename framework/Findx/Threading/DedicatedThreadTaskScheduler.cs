#nullable enable
using System.Threading.Tasks;

namespace Findx.Threading;

/// <summary>
/// 专用线程任务调度程序
/// </summary>
internal sealed  class DedicatedThreadTaskScheduler : TaskScheduler
{
    /// <summary>
    /// 任务集合
    /// </summary>
    private readonly BlockingCollection<Task> _tasks = new();
    
    /// <summary>
    /// 线程集合
    /// </summary>
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Thread[] _threads;

    /// <summary>
    /// 获取需要执行任务
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Task>? GetScheduledTasks() => _tasks;
    
    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="task"></param>
    protected override void QueueTask(Task task) => _tasks.Add(task);
    
    /// <summary>
    /// 尝试执行任务内联
    /// </summary>
    /// <param name="task"></param>
    /// <param name="taskWasPreviouslyQueued"></param>
    /// <returns></returns>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="threadCount">线程数</param>
    public DedicatedThreadTaskScheduler(int threadCount)
    {
        _threads = new Thread[threadCount];
        for (var index = 0; index < threadCount; index++)
        {
            _threads[index] = new Thread(_ =>
            {
                while (true)
                {
                    TryExecuteTask(_tasks.Take());
                }
            });
        }
        Array.ForEach(_threads, it => it.Start());
    }
}