using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Configuration;

/// <summary>
///     配置中心客户端
/// </summary>
public interface IConfigClient
{
    /// <summary>
    ///     加载配置
    /// </summary>
    /// <returns></returns>
    Task LoadAsync();

    /// <summary>
    ///     配置变更事件
    /// </summary>
    /// <param name="callback"></param>
    void OnConfigDataChange(Func<IEnumerable<ConfigItemDto>, Task> callback);

    #region 属性

    /// <summary>
    ///     应用编号
    /// </summary>
    string AppId { get; }

    /// <summary>
    ///     应用密钥
    /// </summary>
    string AppSecret { get; }

    /// <summary>
    ///     环境变量
    /// </summary>
    string Environment { get; }

    /// <summary>
    ///     客户端编号
    /// </summary>
    string ClientId { get; }

    /// <summary>
    ///     当前数据编号
    /// </summary>
    long CurrentDataVersion { get; set; }

    /// <summary>
    ///     服务节点
    /// </summary>
    string Servers { get; }

    /// <summary>
    ///     当前使用服务
    /// </summary>
    string CurrentServer { get; set; }

    #endregion

    #region 获取配置值

    /// <summary>
    ///     获取配置值
    /// </summary>
    /// <param name="key"></param>
    string this[string key] { get; }

    /// <summary>
    ///     获取配置值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string Get(string key);

    #endregion
}