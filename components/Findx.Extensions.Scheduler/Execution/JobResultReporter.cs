using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行结果上报器：把 JobResultReport 回传给调度端。
///     <para>只依赖上行协议通道 <see cref="ISchedulerMasterChannel" />，不感知 HTTP / RPC 差异，
///     也不持有任何数据库原语。地址优先用分发包携带的 MasterAddress，失败再尝试配置的 MasterEndpoints；
///     两者都不可用时放弃上报，由调度端的执行账本超时兜底接管。</para>
/// </summary>
public class JobResultReporter
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly ISchedulerMasterChannel _channel;
    private readonly JobResultShipper _shipper;
    private readonly ILogger<JobResultReporter> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobResultReporter(IOptions<SchedulerOptions> options, ISchedulerMasterChannel channel,
        JobResultShipper shipper, ILogger<JobResultReporter> logger)
    {
        _options = options;
        _channel = channel;
        _shipper = shipper;
        _logger = logger;
    }

    /// <summary>
    ///     上报一次执行结果
    /// </summary>
    /// <param name="request"></param>
    /// <param name="success"></param>
    /// <param name="error"></param>
    /// <param name="message"></param>
    /// <param name="elapsedMs"></param>
    /// <param name="logs">执行过程日志（执行端采集，调度端截断后落库）</param>
    /// <param name="cancellationToken"></param>
    public async Task ReportAsync(DispatchRequest request, bool success, string error, string message, long elapsedMs,
        string logs = null, CancellationToken cancellationToken = default)
    {
        if (request == null) return;

        var report = new JobResultReport
        {
            TraceId = request.TraceId,
            JobId = request.JobId,
            JobName = request.JobName,
            IsSuccess = success,
            Error = error,
            Message = message,
            ElapsedMs = elapsedMs,
            Logs = logs,
            WorkerAddress = _options.Value.NodeAddress
        };

        //  批量上报：入队即返回，由 JobResultShipper 按窗口合并成一次上行请求
        //  关闭批量或队列已满时回落逐条上报，结果不能丢
        if (_shipper.TryShip(report, request.MasterAddress)) return;

        await ReportDirectAsync(report, request.MasterAddress, cancellationToken);
    }

    /// <summary>
    ///     逐条上报（批量关闭 / 结果队列满时的回退路径）
    /// </summary>
    /// <param name="report"></param>
    /// <param name="masterAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ReportDirectAsync(JobResultReport report, string masterAddress, CancellationToken cancellationToken = default)
    {
        if (await _channel.ReportResultAsync(report, masterAddress, cancellationToken)) return;

        _logger.LogError("任务 [{JobName}] TraceId={TraceId} 回调调度端失败（协议 {Protocol}），交由执行账本超时兜底",
            report.JobName, report.TraceId, _channel.Protocol);
    }
}
