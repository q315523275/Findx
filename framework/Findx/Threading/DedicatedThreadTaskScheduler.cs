#nullable enable
using System.Threading.Tasks;

namespace Findx.Threading;

/// <summary>
///     专用线程任务调度程序
/// </summary>
internal sealed  class DedicatedThreadTaskScheduler : TaskScheduler, IDisposable
{
    /// <summary>
    ///     任务集合
    /// </summary>
    private readonly BlockingCollection<Task> _tasks = new();
    
    /// <summary>
    ///     线程集合
    /// </summary>
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Thread[] _threads;

    /// <summary>
    ///     获取需要执行任务
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Task>? GetScheduledTasks() => _tasks.ToArray();
    
    /// <summary>
    ///     添加任务
    /// </summary>
    /// <param name="task"></param>
    protected override void QueueTask(Task task)
    {
        //  集合已标记完成（Dispose 后）不再接收新任务
        if (_tasks.IsAddingCompleted)
            throw new InvalidOperationException("DedicatedThreadTaskScheduler has been disposed");
        _tasks.Add(task);
    }
    
    /// <summary>
    ///     尝试执行任务内联
    /// </summary>
    /// <param name="task"></param>
    /// <param name="taskWasPreviouslyQueued"></param>
    /// <returns></returns>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="threadCount">线程数</param>
    public DedicatedThreadTaskScheduler(int threadCount)
    {
        _threads = new Thread[threadCount];
        for (var index = 0; index < threadCount; index++)
        {
            var thread = new Thread(_ =>
            {
                while (true)
                {
                    try
                    {
                        var task = _tasks.Take();
                        TryExecuteTask(task);
                    }
                    catch (InvalidOperationException)
                    {
                        //  CompleteAdding 后 Take 抛出，线程正常退出
                        break;
                    }
                }
            })
            {
                //  后台线程：即使忘记 Dispose 也不会阻止进程退出
                IsBackground = true
            };
            _threads[index] = thread;
        }
        Array.ForEach(_threads, it => it.Start());
    }

    /// <summary>
    ///     停止调度并等待所有线程退出
    /// </summary>
    public void Dispose()
    {
        _tasks.CompleteAdding();
        foreach (var thread in _threads)
        {
            thread.Join(TimeSpan.FromSeconds(2));
        }
    }
}