using Findx.Jobs;
using Findx.Jobs.Client;
using Findx.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Findx.UnitTests;

/// <summary>
///     JobExecutor 回归测试（修复：作业失败静默记成功 → JobResult.FromException + 重新抛出）
/// </summary>
public class JobExecutorTests
{
    private sealed class FailingJob : IJob
    {
        public Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("job-boom");
    }

    private sealed class OkJob : IJob
    {
        public Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(JobResult.Success);
    }

    [Fact]
    public async Task ExecuteAsync_JobThrows_SetsFailedResultAndRethrows()
    {
        var workSet = new JobWorkSet();
        workSet[typeof(FailingJob).FullName!] = typeof(FailingJob);

        var services = new ServiceCollection();
        services.AddTransient<FailingJob>();
        var provider = services.BuildServiceProvider();

        var context = new JobExecuteContext(provider, new JobExecuteInfo { FullName = typeof(FailingJob).FullName, Name = "t", RunTime = DateTimeOffset.Now });

        var executor = new JobExecutor(workSet, NullLogger<JobExecutor>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(context));

        Assert.Equal("job-boom", ex.Message);
        // 修复前：异常被吞、JobResult 保持 IsSuccess=true；修复后：FromException + throw
        Assert.False(context.JobResult.IsSuccess);
        Assert.NotNull(context.JobResult.Error);
    }

    [Fact]
    public async Task ExecuteAsync_JobSucceeds_SetsSuccessResult()
    {
        var workSet = new JobWorkSet();
        workSet[typeof(OkJob).FullName!] = typeof(OkJob);

        var services = new ServiceCollection();
        services.AddTransient<OkJob>();
        var provider = services.BuildServiceProvider();

        var context = new JobExecuteContext(provider, new JobExecuteInfo { FullName = typeof(OkJob).FullName, Name = "ok", RunTime = DateTimeOffset.Now });

        var executor = new JobExecutor(workSet, NullLogger<JobExecutor>.Instance);

        await executor.ExecuteAsync(context);

        Assert.True(context.JobResult.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownJob_ReturnsSilently()
    {
        var workSet = new JobWorkSet();
        var provider = new ServiceCollection().BuildServiceProvider();
        var context = new JobExecuteContext(provider, new JobExecuteInfo { FullName = "No.Such.Job", Name = "x", RunTime = DateTimeOffset.Now });

        var executor = new JobExecutor(workSet, NullLogger<JobExecutor>.Instance);

        await executor.ExecuteAsync(context);
        Assert.True(context.JobResult.IsSuccess);
    }
}
