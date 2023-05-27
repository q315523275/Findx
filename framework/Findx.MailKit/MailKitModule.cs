using System.ComponentModel;
using Findx.Email;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.MailKit
{
    /// <summary>
    ///     Findx-MailKit邮件模块
    /// </summary>
    [Description("Findx-MailKit邮件模块")]
    public class MailKitModule : FindxModule
    {
        /// <summary>
        ///     模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        ///     模块编号
        /// </summary>
        public override int Order => 60;

        /// <summary>
        ///     配置模块服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.Replace(new ServiceDescriptor(typeof(IEmailSender), typeof(MailKitEmailSender),
                ServiceLifetime.Singleton));

            return services;
        }
    }
}