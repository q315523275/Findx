using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Findx.Sms
{
    public class NullSmsSender : ISmsSender
    {
        private readonly ILogger<NullSmsSender> _logger;

        public NullSmsSender(ILogger<NullSmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(SmsMessage sms)
        {
            _logger.LogWarning($"Unable to find SMS provider");

            return Task.CompletedTask;
        }
    }
}
