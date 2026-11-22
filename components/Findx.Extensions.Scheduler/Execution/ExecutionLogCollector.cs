using System.Text;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Jobs.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行日志采集器：仅在「某一次执行」的异步上下文内生效，按 TraceId 收集日志条目。
///     <para>两种出口：① 环形缓冲保留最近若干条，执行结束后取尾部随结果回调（快速查看）；
///     ② 达到批量阈值即通过 <see cref="ExecutionLogChunk"/> 交给上报队列（日志量大时边执行边上送，不占回调体积）。</para>
///     <para>以 <see cref="AsyncLocal{T}"/> 挂当前执行作用域，并发执行互不串日志；无活跃作用域时全部空转。</para>
/// </summary>
public sealed class ExecutionLogCollector
{
    /// <summary>
    ///     单次执行环形缓冲保留的日志条数
    /// </summary>
    internal const int RingCapacity = 300;

    /// <summary>
    ///     单个上报分片的最大字符数（超长保留尾部）
    /// </summary>
    internal const int MaxChunkChars = 8000;

    /// <summary>
    ///     单条日志最大字符数
    /// </summary>
    internal const int MaxEntryChars = 800;

    /// <summary>
    ///     每个上报分片包含的日志条数
    /// </summary>
    internal const int ChunkEntries = 50;

    private readonly AsyncLocal<ExecutionLogScope> _current = new();
    private readonly ExecutionLogMasker _masker;

    /// <summary>
    ///     Ctor（无配置时：日志原样采集，不脱敏）
    /// </summary>
    public ExecutionLogCollector()
    {
    }

    /// <summary>
    ///     Ctor（按配置决定是否启用脱敏）
    /// </summary>
    public ExecutionLogCollector(IOptions<SchedulerOptions> options)
    {
        var opts = options.Value;
        _masker = opts.LogMaskingEnabled ? new ExecutionLogMasker(true, opts.LogMaskPatterns) : null;
    }

    /// <summary>
    ///     当前异步上下文是否有活跃采集作用域
    /// </summary>
    public bool IsActive => _current.Value != null;

    /// <summary>
    ///     开启一次采集
    /// </summary>
    /// <param name="traceId">执行链路编号</param>
    /// <param name="jobId">任务编号（随日志分片上报）</param>
    /// <param name="sink">分片出口（为空时只保留环形缓冲，不走队列上报）</param>
    /// <returns>采集作用域，可作为 <see cref="IJobLogRecorder"/> 注入执行上下文</returns>
    public ExecutionLogScope Begin(string traceId, long jobId = 0, Action<ExecutionLogChunk> sink = null)
    {
        var scope = new ExecutionLogScope(this, traceId, jobId, sink, _current.Value, _masker);
        _current.Value = scope;
        return scope;
    }

    /// <summary>
    ///     运行时写入一条日志（无活跃作用域时静默丢弃）
    /// </summary>
    public void Write(LogLevel level, string category, string message, Exception exception)
    {
        _current.Value?.Add(level, category, message, exception);
    }

    /// <summary>
    ///     退出作用域
    /// </summary>
    internal void Exit(ExecutionLogScope scope, ExecutionLogScope previous)
    {
        if (_current.Value == scope) _current.Value = previous;
    }

}

/// <summary>
///     一次执行的日志作用域：既是 ILogger 旁路采集目标，也是 <see cref="IJobLogRecorder"/> 的实现
/// </summary>
public sealed class ExecutionLogScope : IJobLogRecorder, IDisposable
{
    private readonly ExecutionLogCollector _owner;
    private readonly ExecutionLogScope _previous;
    private readonly ExecutionLogMasker _masker;
    private readonly string _traceId;
    private readonly long _jobId;
    private readonly Action<ExecutionLogChunk> _sink;
    private readonly List<string> _entries = new();
    private readonly List<string> _pending = new();
    private long _total;
    private int _sequence;
    private bool _finalFlushed;
    private bool _disposed;

    internal ExecutionLogScope(ExecutionLogCollector owner, string traceId, long jobId,
        Action<ExecutionLogChunk> sink, ExecutionLogScope previous, ExecutionLogMasker masker = null)
    {
        _owner = owner;
        _traceId = traceId;
        _jobId = jobId;
        _sink = sink;
        _previous = previous;
        _masker = masker;
    }

    /// <summary>
    ///     手动补一条日志（调度侧关键事件或 Job 主动记录）
    /// </summary>
    public void Write(string message)
    {
        Add(LogLevel.Information, "scheduler", message, null);
    }

    /// <summary>
    ///     <see cref="IJobLogRecorder"/> 实现：Job 通过执行上下文主动记录过程信息
    /// </summary>
    public void Log(LogLevel level, string message, Exception exception = null)
    {
        Add(level, "job", message, exception);
    }

    /// <summary>
    ///     添加一条日志（日志提供器写入路径）
    /// </summary>
    internal void Add(LogLevel level, string category, string message, Exception exception)
    {
        if (_disposed) return;

        Interlocked.Increment(ref _total);

        var line = Format(level, category, message, exception);
        if (_masker != null) line = _masker.Mask(line);

        List<string> chunk = null;
        lock (_entries)
        {
            //  环形缓冲：只留最近 N 条
            _entries.Add(line);
            if (_entries.Count > ExecutionLogCollector.RingCapacity) _entries.RemoveAt(0);

            //  分片缓冲：达到阈值整片交出去，长任务的日志不必等执行结束才可见
            _pending.Add(line);
            if (_pending.Count >= ExecutionLogCollector.ChunkEntries)
            {
                chunk = FlushPendingLocked();
            }
        }

        Emit(chunk, false);
    }

    /// <summary>
    ///     结束采集：把剩余日志作为最后一片上报（幂等）
    /// </summary>
    public void FlushFinal()
    {
        if (_finalFlushed) return;

        List<string> chunk;
        lock (_entries)
        {
            _finalFlushed = true;
            chunk = FlushPendingLocked();
        }

        Emit(chunk, true);
    }

    /// <summary>
    ///     渲染为可落库的文本（超长时保留尾部）
    /// </summary>
    /// <param name="maxChars">最大字符数，&lt;=0 表示不截断</param>
    public string Render(int maxChars)
    {
        string text;
        lock (_entries)
        {
            var builder = new StringBuilder(_entries.Count * 64);
            var omitted = _total - _entries.Count;
            if (omitted > 0) builder.Append("…省略前 ").Append(omitted).Append(" 条，仅保留最近 ").Append(_entries.Count).Append(" 条").Append('\n');
            foreach (var entry in _entries) builder.Append(entry).Append('\n');
            text = builder.ToString();
        }

        if (maxChars > 0 && text.Length > maxChars)
            text = "…日志过长，仅保留尾部…\n" + text[^maxChars..];

        return text;
    }

    /// <summary>
    ///     释放作用域（剩余日志仍会补发最后一片）
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        FlushFinal();
        _disposed = true;
        _owner.Exit(this, _previous);
    }

    private List<string> FlushPendingLocked()
    {
        if (_pending.Count == 0) return null;

        var list = new List<string>(_pending);
        _pending.Clear();
        return list;
    }

    private void Emit(List<string> lines, bool isFinal)
    {
        if (lines == null || lines.Count == 0 || _sink == null) return;

        var content = string.Join('\n', lines);
        if (content.Length > ExecutionLogCollector.MaxChunkChars)
            content = "…本片日志过长，仅保留尾部…\n" + content[^ExecutionLogCollector.MaxChunkChars..];

        var chunk = new ExecutionLogChunk
        {
            TraceId = _traceId,
            JobId = _jobId,
            Sequence = Interlocked.Increment(ref _sequence) - 1,
            Content = content,
            IsFinal = isFinal,
            CreatedTime = DateTime.Now
        };

        _sink(chunk);
    }

    private static string Format(LogLevel level, string category, string message, Exception exception)
    {
        var builder = new StringBuilder(64);
        builder.Append(DateTime.Now.ToString("HH:mm:ss.fff")).Append(' ')
            .Append(LevelText(level)).Append(' ')
            .Append(ShortCategory(category)).Append(' ')
            .Append(message ?? string.Empty);

        if (exception != null) builder.Append(" | ").Append(exception.GetType().Name).Append(": ").Append(exception.Message);

        var line = builder.ToString();
        return line.Length > ExecutionLogCollector.MaxEntryChars ? line[..ExecutionLogCollector.MaxEntryChars] + "…" : line;
    }

    private static string LevelText(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => "???"
        };
    }

    private static string ShortCategory(string category)
    {
        if (string.IsNullOrEmpty(category)) return "-";
        var index = category.LastIndexOf('.');
        return index >= 0 && index < category.Length - 1 ? category[(index + 1)..] : category;
    }
}

/// <summary>
///     执行日志提供器：把所有日志调用旁路采集进当前执行作用域。
///     <para>标准 <see cref="ILoggerProvider"/>，不替换任何既有提供器；无活跃作用域时空转。</para>
/// </summary>
public sealed class ExecutionLogProvider : ILoggerProvider
{
    private readonly ExecutionLogCollector _collector;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ExecutionLogProvider(ExecutionLogCollector collector)
    {
        _collector = collector;
    }

    /// <summary>
    ///     创建日志记录器
    /// </summary>
    public ILogger CreateLogger(string categoryName) => new ExecutionLogger(categoryName, _collector);

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
    }
}

/// <summary>
///     采集型日志记录器：仅写采集器，不负责输出
/// </summary>
internal sealed class ExecutionLogger : ILogger
{
    private readonly string _category;
    private readonly ExecutionLogCollector _collector;

    public ExecutionLogger(string category, ExecutionLogCollector collector)
    {
        _category = category;
        _collector = collector;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && _collector.IsActive;

    public IDisposable BeginScope<TState>(TState state) => ExecutionNullScope.Instance;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!_collector.IsActive) return;

        _collector.Write(logLevel, _category, formatter?.Invoke(state, exception) ?? string.Empty, exception);
    }
}

/// <summary>
///     空作用域（采集器不维护日志作用域栈）
/// </summary>
internal sealed class ExecutionNullScope : IDisposable
{
    public static readonly ExecutionNullScope Instance = new();

    public void Dispose()
    {
    }
}
