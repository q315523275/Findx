using Findx.Modularity;
using System.ComponentModel;

namespace Findx.MailKit
{
    [Description("Findex-MailKit邮件模块")]
    public class MailKitModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 20;
    }
}
