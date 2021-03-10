using System.Collections.Generic;

namespace Findx.Sms
{
    public class SmsMessage
    {
        public string Phone { get; }

        public string Text { get; }

        public IDictionary<string, object> Parameter { get; }

        public SmsMessage(string phone, string text)
        {
            Check.NotNull(phone, nameof(phone));
            Check.NotNull(text, nameof(text));

            Phone = phone;
            Text = text;

            Parameter = new Dictionary<string, object>();
        }
    }
}
