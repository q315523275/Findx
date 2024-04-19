using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Zx;

/// <summary>
///     字符串进程扩展
/// </summary>
public static class StringProcessExtensions
{
    /// <summary>
    ///     获取命令执行等待器
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static TaskAwaiter<string> GetAwaiter(this string command)
    {
        return ProcessCommand(command).GetAwaiter();
    }

    /// <summary>
    ///     获取命令执行等待器
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    public static TaskAwaiter GetAwaiter(this IEnumerable<string> commands)
    {
        return ProcessCommands().GetAwaiter();

        async Task ProcessCommands()
        {
            await Task.WhenAll(commands.Select(ProcessCommand));
        }
    }

    /// <summary>
    ///     执行进程命令
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private static Task<string> ProcessCommand(string command)
    {
        return TryChangeDirectory(command) ? Task.FromResult("") : Env.Process(command);
    }

    /// <summary>
    ///     使用进程命令尝试更改目录
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private static bool TryChangeDirectory(string command)
    {
        if (command.StartsWith("cd ") || command.StartsWith("chdir "))
        {
            var path = Regex.Replace(command, "^cd|^chdir", "").Trim();
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, path);
            return true;
        }

        return false;
    }
}