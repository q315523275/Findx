using Findx.Modularity;
using System.ComponentModel;

namespace Findx.WebApiClient
{
    [Description("Findx-WebApiClient模块")]
    public class WebApiClientModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 30;
    }
}
