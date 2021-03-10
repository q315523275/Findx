using Microsoft.Extensions.Configuration;
using System;

namespace Findx.Configuration
{
    public static class FindxConfigurationExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the Mixos Configuration.
        /// </summary>
        /// <param name="configurationBuilder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddFindx(this IConfigurationBuilder configurationBuilder, FindxConfigurationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            configurationBuilder.Add(new FindxConfigurationSource(options));
            return configurationBuilder;
        }
    }
}
