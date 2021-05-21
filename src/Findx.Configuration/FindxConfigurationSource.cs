using Microsoft.Extensions.Configuration;

namespace Findx.Configuration
{
    internal class FindxConfigurationSource : IConfigurationSource
    {
        public FindxConfigurationSource()
        {
        }

        public FindxConfigurationSource(FindxConfigurationOptions options)
        {
            Options = options;
        }
        public FindxConfigurationOptions Options { set; get; }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new FindxConfigurationProvider(Options);
        }
    }
}
