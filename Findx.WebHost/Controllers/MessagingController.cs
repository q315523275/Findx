using Findx.Messaging;
using Findx.WebHost.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class MessagingController : ControllerBase
    {
        private readonly IMessageSender messageSender;
        private readonly IMessageNotifySender messageNotifySender;

        public MessagingController(IMessageSender messageSender, IMessageNotifySender messageNotifySender)
        {
            this.messageSender = messageSender;
            this.messageNotifySender = messageNotifySender;
        }

        [HttpGet]
        public async Task<IActionResult> Send()
        {
            await messageNotifySender.PublishAsync(new PayedOrderCommand(0));
            return Ok("1");
        }
        [HttpGet]
        public async Task<IActionResult> Notify()
        {
            await messageSender.SendAsync(new CancelOrderCommand(0));
            return Ok("1");
        }
    }
}
