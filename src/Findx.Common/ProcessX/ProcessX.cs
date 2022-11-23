#nullable enable
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Findx.ProcessX;

// https://github.com/Cysharp/ProcessX/blob/master/src/ProcessX/ProcessX.cs

/// <summary>
/// 进程增强X
/// </summary>
public static class ProcessX
{
    /// <summary>
    /// 可接受的退出代码
    /// </summary>
    public static IReadOnlyList<int> AcceptableExitCodes { get; set; } = new[] { 0 };

    /// <summary>
    /// 是否无效退出代码
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    private static bool IsInvalidExitCode(Process process)
    {
        return !AcceptableExitCodes.Any(x => x == process.ExitCode);
    }

    /// <summary>
    /// 格式化转换执行命令
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private static (string fileName, string? arguments) ParseCommand(string command)
    {
        var cmdBegin = command.IndexOf(' ');
        if (cmdBegin == -1)
        {
            return (command, null);
        }

        var fileName = command.Substring(0, cmdBegin);
        var arguments = command.Substring(cmdBegin + 1, command.Length - (cmdBegin + 1));
        return (fileName, arguments);
    }

    /// <summary>
    /// 设置可重定向进程
    /// </summary>
    /// <param name="processStartInfo"></param>
    /// <param name="redirectStandardInput"></param>
    /// <returns></returns>
    private static Process SetupRedirectableProcess(ref ProcessStartInfo processStartInfo, bool redirectStandardInput)
    {
        // override settings.
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;
        processStartInfo.ErrorDialog = false;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = redirectStandardInput;

        var process = new Process()
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true,
        };

        return process;
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="workingDirectory">执行目录</param>
    /// <param name="environmentVariable">环境变量字典</param>
    /// <param name="encoding">编码</param>
    /// <returns></returns>
    public static ProcessAsyncEnumerable StartAsync(string command, string? workingDirectory = null, IDictionary<string, string>? environmentVariable = null, Encoding? encoding = null)
    {
        var (fileName, arguments) = ParseCommand(command);
        return StartAsync(fileName, arguments, workingDirectory, environmentVariable, encoding);
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="arguments">参数</param>
    /// <param name="workingDirectory">执行目录</param>
    /// <param name="environmentVariable">环境变量字典</param>
    /// <param name="encoding">编码</param>
    /// <returns></returns>
    public static ProcessAsyncEnumerable StartAsync(string fileName, string? arguments, string? workingDirectory = null, IDictionary<string, string>? environmentVariable = null, Encoding? encoding = null)
    {
        var pi = new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = arguments,
        };

        if (workingDirectory != null)
        {
            pi.WorkingDirectory = workingDirectory;
        }

        if (environmentVariable != null)
        {
            foreach (var item in environmentVariable)
            {
                pi.EnvironmentVariables[item.Key] = item.Value;
            }
        }

        if (encoding != null)
        {
            pi.StandardOutputEncoding = encoding;
            pi.StandardErrorEncoding = encoding;
        }

        return StartAsync(pi);
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="processStartInfo">进行信息</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ProcessAsyncEnumerable StartAsync(ProcessStartInfo processStartInfo)
    {
        var process = SetupRedirectableProcess(ref processStartInfo, false);

        var outputChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
        });

        var errorList = new List<string>();

        var waitOutputDataCompleted = new TaskCompletionSource<object?>();

        void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                outputChannel.Writer.TryWrite(e.Data);
            }
            else
            {
                waitOutputDataCompleted.TrySetResult(null);
            }
        }

        process.OutputDataReceived += OnOutputDataReceived;

        var waitErrorDataCompleted = new TaskCompletionSource<object?>();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (errorList)
                {
                    errorList.Add(e.Data);
                }
            }
            else
            {
                waitErrorDataCompleted.TrySetResult(null);
            }
        };

        process.Exited += async (_, _) =>
        {
            await waitErrorDataCompleted.Task.ConfigureAwait(false);

            if (errorList.Count == 0)
            {
                await waitOutputDataCompleted.Task.ConfigureAwait(false);
            }
            else
            {
                process.OutputDataReceived -= OnOutputDataReceived;
            }

            if (IsInvalidExitCode(process))
            {
                outputChannel.Writer.TryComplete(new ProcessErrorException(process.ExitCode, errorList.ToArray()));
            }
            else
            {
                if (errorList.Count == 0)
                {
                    outputChannel.Writer.TryComplete();
                }
                else
                {
                    outputChannel.Writer.TryComplete(new ProcessErrorException(process.ExitCode, errorList.ToArray()));
                }
            }
            Console.WriteLine("执行进程退出了");
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Can't start process. FileName:" + processStartInfo.FileName +
                                                ", Arguments:" + processStartInfo.Arguments);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return new ProcessAsyncEnumerable(process, outputChannel.Reader);
    }

    /// <summary>
    /// 获取双异步可迭代非泛型集合
    /// </summary>
    /// <param name="command"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="environmentVariable"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static (Process Process, ProcessAsyncEnumerable StdOut, ProcessAsyncEnumerable StdError) GetDualAsyncEnumerable(string command, string? workingDirectory = null, IDictionary<string, string>? environmentVariable = null, Encoding? encoding = null)
    {
        var (fileName, arguments) = ParseCommand(command);
        return GetDualAsyncEnumerable(fileName, arguments, workingDirectory, environmentVariable, encoding);
    }

    /// <summary>
    /// 获取双异步可迭代非泛型集合
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="environmentVariable"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static (Process Process, ProcessAsyncEnumerable StdOut, ProcessAsyncEnumerable StdError) GetDualAsyncEnumerable(string fileName, string? arguments, string? workingDirectory = null, IDictionary<string, string>? environmentVariable = null, Encoding? encoding = null)
    {
        var pi = new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = arguments,
        };

        if (workingDirectory != null)
        {
            pi.WorkingDirectory = workingDirectory;
        }

        if (environmentVariable != null)
        {
            foreach (var item in environmentVariable)
            {
                pi.EnvironmentVariables.Add(item.Key, item.Value);
            }
        }

        if (encoding != null)
        {
            pi.StandardOutputEncoding = encoding;
            pi.StandardErrorEncoding = encoding;
        }

        return GetDualAsyncEnumerable(pi);
    }

    /// <summary>
    /// 获取双异步可迭代非泛型集合
    /// </summary>
    /// <param name="processStartInfo"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static (Process Process, ProcessAsyncEnumerable StdOut, ProcessAsyncEnumerable StdError) GetDualAsyncEnumerable(ProcessStartInfo processStartInfo)
    {
        var process = SetupRedirectableProcess(ref processStartInfo, true);

        var outputChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
        });

        var errorChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
        });

        var waitOutputDataCompleted = new TaskCompletionSource<object?>();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                outputChannel.Writer.TryWrite(e.Data);
            }
            else
            {
                waitOutputDataCompleted.TrySetResult(null);
            }
        };

        var waitErrorDataCompleted = new TaskCompletionSource<object?>();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errorChannel.Writer.TryWrite(e.Data);
            }
            else
            {
                waitErrorDataCompleted.TrySetResult(null);
            }
        };

        process.Exited += async (_, _) =>
        {
            await waitErrorDataCompleted.Task.ConfigureAwait(false);
            await waitOutputDataCompleted.Task.ConfigureAwait(false);

            if (IsInvalidExitCode(process))
            {
                errorChannel.Writer.TryComplete();
                outputChannel.Writer.TryComplete(new ProcessErrorException(process.ExitCode, Array.Empty<string>()));
            }
            else
            {
                errorChannel.Writer.TryComplete();
                outputChannel.Writer.TryComplete();
            }
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Can't start process. FileName:" + processStartInfo.FileName +
                                                ", Arguments:" + processStartInfo.Arguments);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // error itertor does not handle process itself.
        return (process, new ProcessAsyncEnumerable(process, outputChannel.Reader),
            new ProcessAsyncEnumerable(null, errorChannel.Reader));
    }
    
    /// <summary>
    /// 开始执行进程命令并返回Binary格式数据
    /// </summary>
    /// <param name="command"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="environmentVariable"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static Task<byte[]> StartReadBinaryAsync(string command, string? workingDirectory = null, IDictionary<string, string>? environmentVariable = null, Encoding? encoding = null)
    {
        var (fileName, arguments) = ParseCommand(command);
        return StartReadBinaryAsync(fileName, arguments, workingDirectory, environmentVariable, encoding);
    }

    /// <summary>
    /// 开始执行进程命令并返回Binary格式数据
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="environmentVariable"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static Task<byte[]> StartReadBinaryAsync(string fileName, string? arguments, string? workingDirectory = null, IDictionary<string, string>? environmentVariable = null, Encoding? encoding = null)
    {
        var pi = new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = arguments,
        };

        if (workingDirectory != null)
        {
            pi.WorkingDirectory = workingDirectory;
        }

        if (environmentVariable != null)
        {
            foreach (var item in environmentVariable)
            {
                pi.EnvironmentVariables.Add(item.Key, item.Value);
            }
        }

        if (encoding != null)
        {
            pi.StandardOutputEncoding = encoding;
            pi.StandardErrorEncoding = encoding;
        }

        return StartReadBinaryAsync(pi);
    }

    /// <summary>
    /// 开始执行进程命令并返回Binary格式数据
    /// </summary>
    /// <param name="processStartInfo"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Task<byte[]> StartReadBinaryAsync(ProcessStartInfo processStartInfo)
    {
        var process = SetupRedirectableProcess(ref processStartInfo, false);

        var errorList = new List<string>();

        var cts = new CancellationTokenSource();
        var resultTask = new TaskCompletionSource<byte[]>();
        var readTask = new TaskCompletionSource<byte[]?>();

        var waitErrorDataCompleted = new TaskCompletionSource<object?>();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (errorList)
                {
                    errorList.Add(e.Data);
                }
            }
            else
            {
                waitErrorDataCompleted.TrySetResult(null);
            }
        };

        process.Exited += async (_, _) =>
        {
            Console.WriteLine("xxxxxxxxxxxxxx");
            
            await waitErrorDataCompleted.Task.ConfigureAwait(false);

            if (errorList.Count == 0 && !IsInvalidExitCode(process))
            {
                var resultBin = await readTask.Task.ConfigureAwait(false);
                if (resultBin != null)
                {
                    resultTask.TrySetResult(resultBin);
                    return;
                }
            }

            cts.Cancel();

            resultTask.TrySetException(new ProcessErrorException(process.ExitCode, errorList.ToArray()));
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Can't start process. FileName:" + processStartInfo.FileName +
                                                ", Arguments:" + processStartInfo.Arguments);
        }

        RunAsyncReadFully(process.StandardOutput.BaseStream, readTask, cts.Token);
        process.BeginErrorReadLine();

        return resultTask.Task;
    }

    private static async void RunAsyncReadFully(Stream stream, TaskCompletionSource<byte[]?> completion, CancellationToken cancellationToken)
    {
        try
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms, 81920, cancellationToken);
            var result = ms.ToArray();
            completion.TrySetResult(result);
        }
        catch
        {
            completion.TrySetResult(null);
        }
    }
}