using System.Threading.Tasks;

namespace Findx.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(SmsMessage sms);
    }
}
