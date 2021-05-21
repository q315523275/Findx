using System;
using System.Diagnostics;

namespace Findx
{
    [DebuggerStepThrough]
    public static class Check
    {
        public static T NotNull<T>(T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }
        public static T NotNull<T>(T value, string parameterName, string message)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, message);
            }
            return value;
        }
        public static string NotNull(string value, string parameterName, int maxLength = int.MaxValue, int minLength = 0)
        {
            if (value == null)
            {
                throw new ArgumentException($"{parameterName} can not be null!", parameterName);
            }
            if (value.Length > maxLength)
            {
                throw new ArgumentException($"{parameterName} length must be equal to or lower than {maxLength}!", parameterName);
            }
            if (minLength > 0 && value.Length < minLength)
            {
                throw new ArgumentException($"{parameterName} length must be equal to or bigger than {minLength}!", parameterName);
            }
            return value;
        }

        public static string NotNullOrWhiteSpace(string value, string parameterName, int maxLength = int.MaxValue, int minLength = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} can not be null, empty or white space!", parameterName);
            }
            if (value.Length > maxLength)
            {
                throw new ArgumentException($"{parameterName} length must be equal to or lower than {maxLength}!", parameterName);
            }
            if (minLength > 0 && value.Length < minLength)
            {
                throw new ArgumentException($"{parameterName} length must be equal to or bigger than {minLength}!", parameterName);
            }
            return value;
        }

    }
}
