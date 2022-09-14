using System.Threading.Tasks;

namespace Findx.Sms
{
    /// <summary>
    /// 
    /// </summary>
    public class NullSmsSender : ISmsSender
    {
        private readonly ILogger<NullSmsSender> _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        public NullSmsSender(ILogger<NullSmsSender> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sms"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task SendAsync(SmsMessage sms, CancellationToken token = default)
        {
            _logger.LogWarning($"Unable to find SMS provider");

            return Task.CompletedTask;
        }
    }
}
