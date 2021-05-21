using System;

namespace Findx.Utils
{
    public class DateTimeUtils
    {
        public static TimeSpan ToTimeSpan(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                throw new ArgumentNullException(nameof(time));
            }

            time = time.ToLower();

            if (time.EndsWith("ms"))
            {
                return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 2)), "ms");
            }

            if (time.EndsWith("s"))
            {
                return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 1)), "s");
            }

            if (time.EndsWith("m"))
            {
                return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 1)), "m");
            }

            if (time.EndsWith("h"))
            {
                return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 1)), "h");
            }

            throw new InvalidOperationException("Incorrect format:" + time);
        }

        public static TimeSpan ToTimeSpan(int value, string unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                throw new ArgumentNullException(nameof(unit));
            }

            switch (unit)
            {
                case "ms":
                    return TimeSpan.FromMilliseconds(value);

                case "s":
                    return TimeSpan.FromSeconds(value);

                case "m":
                    return TimeSpan.FromMinutes(value);

                case "h":
                    return TimeSpan.FromHours(value);
                default:
                    break;
            }

            throw new InvalidOperationException("Incorrect unit:" + unit);
        }
    }
}
