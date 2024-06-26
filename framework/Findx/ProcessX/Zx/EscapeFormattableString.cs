// ReSharper disable once CheckNamespace
namespace Zx;

internal static class EscapeFormattableString
{
    internal static string Escape(FormattableString formattableString)
    {
        // already escaped.
        if (formattableString.Format.StartsWith("\"") && formattableString.Format.EndsWith("\""))
        {
            return formattableString.ToString();
        }

        // GetArguments returns inner object[] field, it can modify.
        var args = formattableString.GetArguments();

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] is string)
            {
                args[i] = "\"" + args[i]?.ToString()?.Replace("\"", "\\\"") + "\""; // poor logic
            }
        }

        return formattableString.ToString();
    }
}