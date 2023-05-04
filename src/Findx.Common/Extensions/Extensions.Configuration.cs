using System.Text.RegularExpressions;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 配置
/// </summary>
public static partial class Extensions
{
    // 源
    // https://github.com/henkmollema/ConfigurationPlaceholders/blob/master/src/Microsoft.Extensions.Configuration.Placeholders/ConfigurationExtensions.cs

    /// <summary>
    ///     A regex which matches tokens in the following format: ${Item:Sub1:Sub2}.
    /// </summary>
    private static readonly Regex ConfigPlaceholderRegex = new(@"\$\{([A-Za-z:_]+?)\}");

    /// <summary>
    ///     Replaces the placeholders in the specified <see cref="IConfiguration" /> instance.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration" /> instance to replace placeholders in.</param>
    /// <returns>The given <see cref="IConfiguration" /> instance.</returns>
    public static IConfiguration ReplacePlaceholders(this IConfiguration configuration)
    {
        foreach (var kvp in configuration.AsEnumerable())
        {
            if (string.IsNullOrEmpty(kvp.Value))
                // Skip empty configuration values.
                continue;

            // Replace placeholders in the configuration value.
            var result = ConfigPlaceholderRegex.Replace(kvp.Value, match =>
            {
                if (!match.Success)
                    // Return the original value.
                    return kvp.Value;

                if (match.Groups.Count != 2)
                    // There is a match, but somehow no group for the placeholder.
                    throw InvalidPlaceholderException(match.ToString());

                var placeholder = match.Groups[1].Value;
                if (placeholder.StartsWith(":") || placeholder.EndsWith(":"))
                    // Placeholders cannot start or end with a colon.
                    throw InvalidPlaceholderException(placeholder);

                // Return the value in the configuration instance.
                return configuration[placeholder];
            });

            // Replace the value in the configuration instance.
            configuration[kvp.Key] = result;
        }

        return configuration;
    }

    private static Exception InvalidPlaceholderException(string placeholder)
    {
        return new InvalidOperationException($"Invalid configuration placeholder: '{placeholder}'.");
    }
}