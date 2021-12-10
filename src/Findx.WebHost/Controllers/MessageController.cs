using Findx.Messaging;
using Findx.WebHost.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class MessageController : Controller
    {
        /// <summary>
        /// 消息发送(CQRS)示例接口
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        [HttpGet("/message/request")]
        public async Task<string> MessageSend([FromServices] IMessageSender sender)
        {
            var orderId = Findx.Utils.SnowflakeId.Default().NextId();
            var res = await sender.SendAsync(new CancelOrderCommand(orderId));
            return $"orderId:{orderId},result:{res}";
        }

        /// <summary>
        /// 消息通知示例接口
        /// </summary>
        /// <param name="notifySender"></param>
        /// <returns></returns>
        [HttpGet("/message/notify")]
        public async Task<string> MessageNotify([FromServices] IMessageNotifySender notifySender)
        {
            await notifySender.PublishAsync(new PayedOrderCommand(0));
            return "ok";
        }
    }
}
