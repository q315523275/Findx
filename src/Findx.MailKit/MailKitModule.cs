using Findx.Email;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;

namespace Findx.MailKit
{
    [Description("Findex-MailKit邮件模块")]
    public class MailKitModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 20;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.Replace(new ServiceDescriptor(typeof(IEmailSender), typeof(MailKitEmailSender), ServiceLifetime.Singleton));

            return services;
        }
    }
}
