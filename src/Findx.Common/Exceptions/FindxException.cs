using System;

namespace Findx.Exceptions
{
    public class FindxException : Exception
    {
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public FindxException(string code)
        {
            ErrorCode = code;
        }
        public FindxException(string code, string message) : base(message)
        {
            ErrorCode = code;
            ErrorMessage = message;
        }

        public FindxException(string code, string message, Exception exception) : base(message, exception)
        {
            ErrorCode = code;
            ErrorMessage = message + exception.Message;
        }
    }
}
