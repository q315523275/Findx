using Findx.Extensions.Notify.Channels;
using Findx.Extensions.Notify.Scheduling;
using Findx.Extensions.Notify.ServiceDefaults;
using Findx.Extensions.Notify.Storage;
using Findx.Modularity;
using Findx.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.Extensions.Notify;

/// <summary>
///     Findx-消息通知组件模块（配置节 Findx:Notify）
/// </summary>
[Description("Findx-消息通知组件")]
public class NotifyModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 130;

    /// <summary>
    ///     配置服务
    /// </summary>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();

        var configuration = services.GetConfiguration();
        services.Configure<NotifyOptions>(configuration.GetSection("Findx:Notify"));

        var options = new NotifyOptions();
        configuration.GetSection("Findx:Notify").Bind(options);

        //  通道按配置注册：未启用的通道不注册，调用时返回明确提示，避免"已启用但参数没配"的半可用状态
        if (options.WeCom.Enabled) services.AddSingleton<INotifyChannel, WeComNotifyChannel>();
        if (options.DingTalk.Enabled) services.AddSingleton<INotifyChannel, DingTalkNotifyChannel>();
        if (options.Email.Enabled) services.AddSingleton<INotifyChannel, EmailNotifyChannel>();
        if (options.WeChat.Enabled) services.AddSingleton<INotifyChannel, WeChatNotifyChannel>();
        if (options.WeChatMiniProgram.Enabled)
            services.AddSingleton<INotifyChannel, WeChatMiniProgramNotifyChannel>();

        //  路由与超时收敛（同步发送与异步消费共用）
        services.AddSingleton<NotifyChannelDispatcher>();

        //  异步发送队列：业务线程入队即返回，发送与日志落库在后台线程
        services.AddSingleton<NotifyLogStore>();
        services.AddSingleton<NotifyDispatchQueue>();
        services.AddHostedService(sp => sp.GetRequiredService<NotifyDispatchQueue>());

        //  INotifySender 与框架的 IUserNotifier 共用同一个发送器实例（通道路由与日志都在里面）
        services.AddSingleton<NotifySender>();
        services.AddSingleton<INotifySender>(sp => sp.GetRequiredService<NotifySender>());
        services.Replace(ServiceDescriptor.Singleton<IUserNotifier>(sp => sp.GetRequiredService<NotifySender>()));

        return base.ConfigureServices(services);
    }
}
