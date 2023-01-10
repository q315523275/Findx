using System;
using Microsoft.Extensions.Logging;

namespace Findx.Jobs
{
    /// <summary>
    /// 定义一个作业结果
    /// </summary>
    public class JobResult
    {
        /// <summary>
        /// 是否取消
        /// </summary>
        public bool IsCancelled { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public Exception Error { get; set; }
        
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 静态实例
        /// </summary>
        public static readonly JobResult None = new()
        {
            IsSuccess = true,
            Message = string.Empty
        };

        /// <summary>
        /// 静态取消实例
        /// </summary>
        public static readonly JobResult Cancelled = new()
        {
            IsCancelled = true
        };

        /// <summary>
        /// 静态成功实例
        /// </summary>
        public static readonly JobResult Success = new()
        {
            IsSuccess = true
        };

        /// <summary>
        /// 根据异常创建结果实例
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JobResult FromException(Exception exception, string message = null)
        {
            return new JobResult
            {
                Error = exception,
                IsSuccess = false,
                Message = message ?? exception.Message
            };
        }

        /// <summary>
        /// 创建带有消息描述取消实例
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JobResult CancelledWithMessage(string message)
        {
            return new JobResult
            {
                IsCancelled = true,
                Message = message
            };
        }

        /// <summary>
        /// 创建带有消息描述成功实例
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JobResult SuccessWithMessage(string message)
        {
            return new JobResult
            {
                IsSuccess = true,
                Message = message
            };
        }

        /// <summary>
        /// 创建带有消息描述失败实例
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JobResult FailedWithMessage(string message)
        {
            return new JobResult
            {
                IsSuccess = false,
                Message = message
            };
        }
    }

    /// <summary>
    /// 任务结果扩展
    /// </summary>
    public static class JobResultExtensions
    {
        /// <summary>
        /// 框架日志与任务结果整合扩展
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="result"></param>
        /// <param name="jobName"></param>
        public static void LogJobResult(this ILogger logger, JobResult result, string jobName)
        {
            if (result == null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Null job run result for {JobName}", jobName);

                return;
            }

            if (result.IsCancelled)
                logger.LogWarning(result.Error, "Job run {JobName} cancelled: {Message}", jobName, result.Message);
            else if (!result.IsSuccess)
                logger.LogError(result.Error, "Job run {JobName} failed: {Message}", jobName, result.Message);
            else if (!string.IsNullOrEmpty(result.Message))
                logger.LogInformation("Job run {JobName} succeeded: {Message}", jobName, result.Message);
            else if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Job run {JobName} succeeded", jobName);
        }
    }
}

