using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
/// 命令消息执行期
/// </summary>
public interface ICommandHandler<in T> where T : ICommand
{
    /// <summary>
    /// 处理泛型命令消息
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(T command, CancellationToken cancellationToken = default);
}