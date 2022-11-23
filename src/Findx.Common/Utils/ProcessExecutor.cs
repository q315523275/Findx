#nullable enable
using System.Diagnostics;
using System.Threading.Tasks;

namespace Findx.Utils
{
    /// <summary>
    /// 进程执行器
    /// WeihanLi.Common.Helpers
    /// </summary>
    [Obsolete("Please use ProcessX")]
    public sealed class ProcessExecutor : IDisposable
    {
        /// <summary>
        /// 进程执行退出事件
        /// </summary>
        public event EventHandler<int>? OnExited;

        /// <summary>
        /// 进程执行输出事件
        /// </summary>
        public event EventHandler<string>? OnOutputDataReceived;

        /// <summary>
        /// 进程执行错误事件
        /// </summary>
        public event EventHandler<string>? OnErrorDataReceived;

        /// <summary>
        /// 进程对象
        /// </summary>
        private readonly Process _process;

        /// <summary>
        /// 是否执行进程
        /// </summary>
        private bool _started;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="exePath"></param>
        public ProcessExecutor(string exePath) : this(new ProcessStartInfo(exePath))
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="arguments"></param>
        public ProcessExecutor(string exePath, string arguments) : this(new ProcessStartInfo(exePath, arguments))
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="startInfo"></param>
        public ProcessExecutor(ProcessStartInfo startInfo)
        {
            _process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardError = true;
        }

        /// <summary>
        /// 初始化进程执行事件
        /// </summary>
        private void InitializeEvents()
        {
            _process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    OnOutputDataReceived?.Invoke(sender, args.Data);
                }
            };
            _process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    OnErrorDataReceived?.Invoke(sender, args.Data);
                }
            };
            _process.Exited += (sender, _) =>
            {
                if (sender is Process process)
                {
                    OnExited?.Invoke(sender, process.ExitCode);
                }
                else
                {
                    OnExited?.Invoke(sender, _process.ExitCode);
                }
            };
        }

        /// <summary>
        /// 执行进程
        /// </summary>
        private void Start()
        {
            if (_started)
            {
                return;
            }
            _started = true;

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _process.WaitForExit();
        }

        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="input"></param>
        public async Task SendInput(string input)
        {
            try
            {
                await _process.StandardInput.WriteAsync(input!);
            }
            catch (Exception e)
            {
                OnErrorDataReceived?.Invoke(_process, e.ToString());
            }
        }

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            InitializeEvents();
            Start();
            return _process.ExitCode;
        }

        /// <summary>
        /// 异步执行
        /// </summary>
        /// <returns></returns>
        public async Task<int> ExecuteAsync()
        {
            InitializeEvents();
            return await Task.Run(() =>
            {
                Start();
                return _process.ExitCode;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 释放销毁
        /// </summary>
        public void Dispose()
        {
            _process.Dispose();
            OnExited = null;
            OnOutputDataReceived = null;
            OnErrorDataReceived = null;
        }
    }
}
