namespace Findx.Security;

/// <summary>
///     定义功能信息存储器
/// </summary>
/// <typeparam name="TFunction"></typeparam>
public interface IFunctionStore<TFunction>
{
    /// <summary>
    ///     同步功能信息至存储库
    /// </summary>
    /// <param name="functions"></param>
    void SyncToDatabase(IEnumerable<TFunction> functions);

    /// <summary>
    ///     从存储器同步功能信息
    /// </summary>
    IEnumerable<TFunction> GetFromDatabase();
}