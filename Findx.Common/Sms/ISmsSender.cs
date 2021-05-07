using System.Threading;
using System.Threading.Tasks;

namespace Findx.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(SmsMessage sms, CancellationToken token = default);
    }
}
