#nullable enable
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Zx;

/// <summary>
///     脚本扩展
/// </summary>
public static class Env
{
    /// <summary>
    ///     是否冗长执行
    /// </summary>
    public static bool Verbose { get; set; } = true;

    private static string? _shell;
    
    /// <summary>
    ///     Shell
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static string Shell
    {
        get
        {
            if (_shell == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _shell = "cmd /c";
                }
                else
                {
                    if (Which.TryGetPath("bash", out var bashPath))
                    {
                        _shell = bashPath + " -c";
                    }
                    else
                    {
                        throw new InvalidOperationException("shell is not found in PATH, set Env.shell manually.");
                    }
                }
            }

            return _shell;
        }
        set => _shell = value;
    }

    /// <summary>
    ///     终止令牌源
    /// </summary>
    private static readonly Lazy<CancellationTokenSource> TerminateTokenSource = new(() =>
    {
        var source = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => source.Cancel();
        return source;
    });
    
    /// <summary>
    ///     终止令牌
    /// </summary>
    public static CancellationToken TerminateToken => TerminateTokenSource.Value.Token;

    /// <summary>
    ///     工作目录
    /// </summary>
    public static string? WorkingDirectory { get; set; }

    private static readonly Lazy<IDictionary<string, string>> _envvars = new(() => new Dictionary<string, string>());

    /// <summary>
    ///     环境变量
    /// </summary>
    public static IDictionary<string, string> EnvVars => _envvars.Value;

    /// <summary>
    ///     请求
    /// </summary>
    /// <param name="requestUri"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> Fetch(string requestUri)
    {
        return new HttpClient().GetAsync(requestUri);
    }

    /// <summary>
    ///    请求文本
    /// </summary>
    /// <param name="requestUri"></param>
    /// <returns></returns>
    public static Task<string> FetchText(string requestUri)
    {
        return new HttpClient().GetStringAsync(requestUri);
    }

    /// <summary>
    ///     请求字节
    /// </summary>
    /// <param name="requestUri"></param>
    /// <returns></returns>
    public static Task<byte[]> FetchBytes(string requestUri)
    {
        return new HttpClient().GetByteArrayAsync(requestUri);
    }

    /// <summary>
    ///     休眠
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task Sleep(int seconds, CancellationToken cancellationToken = default)
    {
        return Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
    }

    /// <summary>
    ///     休眠
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task Sleep(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        return Task.Delay(timeSpan, cancellationToken);
    }

    /// <summary>
    ///     执行带超时时间
    /// </summary>
    /// <param name="command"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static async Task<string> WithTimeout(FormattableString command, int seconds)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        return (await ProcessStartAsync(EscapeFormattableString.Escape(command), cts.Token)).StdOut;
    }

    /// <summary>
    ///     执行带超时时间
    /// </summary>
    /// <param name="command"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static async Task<(string StdOut, string StdError)> WithTimeout2(FormattableString command, int seconds)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        return (await ProcessStartAsync(EscapeFormattableString.Escape(command), cts.Token));
    }

    /// <summary>
    ///     执行带超时时间
    /// </summary>
    /// <param name="command"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static async Task<string> WithTimeout(FormattableString command, TimeSpan timeSpan)
    {
        using var cts = new CancellationTokenSource(timeSpan);
        return (await ProcessStartAsync(EscapeFormattableString.Escape(command), cts.Token)).StdOut;
    }

    /// <summary>
    ///     执行带超时时间
    /// </summary>
    /// <param name="command"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static async Task<(string StdOut, string StdError)> WithTimeout2(FormattableString command, TimeSpan timeSpan)
    {
        using var cts = new CancellationTokenSource(timeSpan);
        return (await ProcessStartAsync(EscapeFormattableString.Escape(command), cts.Token));
    }

    /// <summary>
    ///     执行带取消令牌
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> WithCancellation(FormattableString command, CancellationToken cancellationToken)
    {
        return (await ProcessStartAsync(EscapeFormattableString.Escape(command), cancellationToken)).StdOut;
    }

    /// <summary>
    ///     执行带取消令牌
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<(string StdOut, string StdError)> WithCancellation2(FormattableString command, CancellationToken cancellationToken)
    {
        return (await ProcessStartAsync(EscapeFormattableString.Escape(command), cancellationToken));
    }

    /// <summary>
    ///     执行进程命令并返回结果
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<string> Run(FormattableString command, CancellationToken cancellationToken = default)
    {
        return Process(EscapeFormattableString.Escape(command), cancellationToken);
    }

    /// <summary>
    ///     执行进程命令并返回结果
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<(string StdOut, string StdError)> Run2(FormattableString command, CancellationToken cancellationToken = default)
    {
        return Process2(EscapeFormattableString.Escape(command), cancellationToken);
    }

    /// <summary>
    ///     执行进程命令并返回结果集合
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<string[]> RunLine(FormattableString command, CancellationToken cancellationToken = default)
    {
        return ProcessLine(EscapeFormattableString.Escape(command), cancellationToken);
    }

    /// <summary>
    ///     执行进程命令并返回结果集合
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<(string[] StdOut, string[] StdError)> RunLine2(FormattableString command, CancellationToken cancellationToken = default)
    {
        return ProcessLine2(EscapeFormattableString.Escape(command), cancellationToken);
    }

    /// <summary>
    ///     推出
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static string Escape(FormattableString command)
    {
        return EscapeFormattableString.Escape(command);
    }

    /// <summary>
    ///     执行进程命令并返回结果
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> Process(string command, CancellationToken cancellationToken = default)
    {
        return (await ProcessStartAsync(command, cancellationToken)).StdOut;
    }

    /// <summary>
    ///     执行进程命令并返回结果
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<(string StdOut, string StdError)> Process2(string command, CancellationToken cancellationToken = default)
    {
        return await ProcessStartAsync(command, cancellationToken);
    }

    /// <summary>
    ///     执行进程命令并返回结果集合
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string[]> ProcessLine(string command, CancellationToken cancellationToken = default)
    {
        return (await ProcessStartListAsync(command, cancellationToken)).StdOut;
    }

    /// <summary>
    ///     执行进程命令并返回结果集合
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<(string[] StdOut, string[] StdError)> ProcessLine2(string command, CancellationToken cancellationToken = default)
    {
        return await ProcessStartListAsync(command, cancellationToken);
    }

    /// <summary>
    ///     忽略异常
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T> Ignore<T>(Task<T> task)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (ProcessErrorException)
        {
            return default!;
        }
    }

    /// <summary>
    ///     读取交互问题
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public static async Task<string> Question(string question)
    {
        Console.WriteLine(question);
        var str = await Console.In.ReadLineAsync();
        return str ?? "";
    }

    /// <summary>
    ///     输出日志
    /// </summary>
    /// <param name="value"></param>
    /// <param name="color"></param>
    public static void Log(object? value, ConsoleColor? color = default)
    {
        if (color != null)
        {
            using (Env.Color(color.Value))
            {
                Console.WriteLine(value);
            }
        }
        else
        {
            Console.WriteLine(value);
        }
    }

    /// <summary>
    ///     输出颜色
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static IDisposable Color(ConsoleColor color)
    {
        var current = Console.ForegroundColor;
        Console.ForegroundColor = color;
        return new ColorScope(current);
    }

    /// <summary>
    ///     执行进程命令并返回执行结果
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="forceSilent"></param>
    /// <returns></returns>
    private static async Task<(string StdOut, string StdError)> ProcessStartAsync(string command, CancellationToken cancellationToken, bool forceSilent = false)
    {
        var cmd = Shell + " \"" + command + "\"";
        var sbOut = new StringBuilder();
        var sbError = new StringBuilder();

        var (_, stdout, stderr) = ProcessX.GetDualAsyncEnumerable(cmd, WorkingDirectory, EnvVars);

        var runStdout = Task.Run(async () =>
        {
            var isFirst = true;
            await foreach (var item in stdout.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!isFirst)
                {
                    sbOut.AppendLine();
                }
                else
                {
                    isFirst = false;
                }

                sbOut.Append(item);

                if (Verbose && !forceSilent)
                {
                    Console.WriteLine(item);
                }
            }
        }, cancellationToken);

        var runStdError = Task.Run(async () =>
        {
            var isFirst = true;
            await foreach (var item in stderr.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!isFirst)
                {
                    sbOut.AppendLine();
                }
                else
                {
                    isFirst = false;
                }

                sbError.Append(item);

                if (Verbose && !forceSilent)
                {
                    Console.WriteLine(item);
                }
            }
        }, cancellationToken);

        await Task.WhenAll(runStdout, runStdError).ConfigureAwait(false);

        return (sbOut.ToString(), sbError.ToString());
    }

    /// <summary>
    ///     执行进程命令并返回执行结果
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="forceSilent">无需打印控制台</param>
    /// <returns></returns>
    private static async Task<(string[] StdOut, string[] StdError)> ProcessStartListAsync(string command, CancellationToken cancellationToken, bool forceSilent = false)
    {
        var cmd = Shell + " \"" + command + "\"";
        var sbOut = new List<string>();
        var sbError = new List<string>();

        var (_, stdout, stderr) = ProcessX.GetDualAsyncEnumerable(cmd, WorkingDirectory, EnvVars);

        var runStdout = Task.Run(async () =>
        {
            await foreach (var item in stdout.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                sbOut.Add(item);

                if (Verbose && !forceSilent)
                {
                    Console.WriteLine(item);
                }
            }
        }, cancellationToken);

        var runStdError = Task.Run(async () =>
        {
            await foreach (var item in stderr.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                sbError.Add(item);

                if (Verbose && !forceSilent)
                {
                    Console.WriteLine(item);
                }
            }
        }, cancellationToken);

        await Task.WhenAll(runStdout, runStdError).ConfigureAwait(false);

        return (sbOut.ToArray(), sbError.ToArray());
    }

    private class ColorScope : IDisposable
    {
        private readonly ConsoleColor _color;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="color"></param>
        public ColorScope(ConsoleColor color)
        {
            _color = color;
        }

        public void Dispose()
        {
            Console.ForegroundColor = _color;
        }
    }
}