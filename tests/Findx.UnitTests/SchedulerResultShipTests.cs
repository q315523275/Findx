using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Execution;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Findx.UnitTests;

/// <summary>
///     结果批量上报回归测试。
///     约束：多条结果合并成一次上行请求；批量关闭（BatchSize=1）或队列满时必须回落逐条上报，结果不能丢。
/// </summary>
public class SchedulerResultShipTests
{
    private static JobResultReport Report(string traceId)
    {
        return new JobResultReport
        {
            TraceId = traceId,
            JobId = 1,
            JobName = "bench-job",
            IsSuccess = true,
            ElapsedMs = 5,
            WorkerAddress = "http://worker1:7101"
        };
    }

    private static JobResultShipper BuildShipper(ISchedulerMasterChannel channel, int batchSize, int queueCapacity = 4096)
    {
        var options = Options.Create(new SchedulerOptions
        {
            ResultShipBatchSize = batchSize,
            ResultShipIntervalMs = 200,
            ResultShipQueueCapacity = queueCapacity
        });

        return new JobResultShipper(options, channel, NullLogger<JobResultShipper>.Instance);
    }

    [Fact]
    public async Task Shipper_MergesMultipleResultsIntoOneRequest()
    {
        var channel = new FakeMasterChannel();
        var shipper = BuildShipper(channel, batchSize: 3);
        await shipper.StartAsync(CancellationToken.None);

        Assert.True(shipper.TryShip(Report("t1"), "http://master:7100"));
        Assert.True(shipper.TryShip(Report("t2"), "http://master:7100"));
        Assert.True(shipper.TryShip(Report("t3"), "http://master:7100"));

        for (var i = 0; i < 100 && channel.Batches.Count == 0; i++) await Task.Delay(20);

        await shipper.StopAsync(CancellationToken.None);

        //  三条结果必须合成一次批量上报，而不是三次逐条回调
        Assert.Single(channel.Batches);
        Assert.Equal(new[] { "t1", "t2", "t3" }, channel.Batches[0]);
        Assert.Equal(0, channel.SingleCalls);
    }

    [Fact]
    public async Task Shipper_GroupsByMasterAddress()
    {
        var channel = new FakeMasterChannel();
        var shipper = BuildShipper(channel, batchSize: 2);
        await shipper.StartAsync(CancellationToken.None);

        shipper.TryShip(Report("t1"), "http://master-a:7100");
        shipper.TryShip(Report("t2"), "http://master-b:7100");

        for (var i = 0; i < 100 && channel.Batches.Count < 2; i++) await Task.Delay(20);

        await shipper.StopAsync(CancellationToken.None);

        //  不同调度端的结果不能混发到同一个请求
        Assert.Equal(2, channel.Batches.Count);
        Assert.Equal(new[] { "http://master-a:7100", "http://master-b:7100" }.OrderBy(x => x), channel.Addresses.OrderBy(x => x));
    }

    [Fact]
    public void Shipper_DisabledWhenBatchSizeIsOne_FallsBackToPerItem()
    {
        var shipper = BuildShipper(new FakeMasterChannel(), batchSize: 1);

        //  返回 false = 调用方必须走逐条上报，不能静默丢弃
        Assert.False(shipper.TryShip(Report("t1"), "http://master:7100"));
    }

    [Fact]
    public void Shipper_QueueFull_ReturnsFalseInsteadOfDropping()
    {
        //  不启动消费线程：写满容量后必须拒绝入队，由调用方回落逐条上报
        var shipper = BuildShipper(new FakeMasterChannel(), batchSize: 10, queueCapacity: 64);

        for (var i = 0; i < 64; i++) Assert.True(shipper.TryShip(Report($"t{i}"), "http://master:7100"));

        Assert.False(shipper.TryShip(Report("t-full"), "http://master:7100"));
    }

    private sealed class FakeMasterChannel : ISchedulerMasterChannel
    {
        public string Protocol => "fake";

        public int SingleCalls { get; private set; }

        public List<List<string>> Batches { get; } = [];

        public List<string> Addresses { get; } = [];

        public Task<bool> HeartbeatAsync(WorkerNodeInfo info, CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> OfflineAsync(WorkerNodeInfo info, CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> ReportResultAsync(JobResultReport report, string preferredMasterAddress, CancellationToken cancellationToken = default)
        {
            SingleCalls++;
            return Task.FromResult(true);
        }

        public Task<bool> ReportResultsAsync(string preferredMasterAddress, IReadOnlyList<JobResultReport> reports,
            CancellationToken cancellationToken = default)
        {
            lock (Batches)
            {
                Addresses.Add(preferredMasterAddress);
                Batches.Add(reports.Select(x => x.TraceId).ToList());
            }

            return Task.FromResult(true);
        }

        public Task<bool> ShipLogsAsync(string preferredMasterAddress, IReadOnlyList<ExecutionLogChunk> chunks,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
