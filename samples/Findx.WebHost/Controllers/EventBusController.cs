using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Events;
using Findx.Utilities;
using Findx.WebHost.EventHandlers;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     事件总线
/// </summary>
[Route("api/eventbus")]
[Description("事件总线"), Tags("事件总线")]
public class EventBusController : ApiControllerBase
{
    /// <summary>
    ///     推送
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("publish")]
    public async Task<string> PublishAsync([FromServices] IEventBus context, CancellationToken cancellationToken)
    {
        var command = new PayedOrderCommand(SnowflakeIdUtility.Default().NextId());
        var orderId = command.OrderId;
        await context.PublishAsync(command, cancellationToken);
        return $"IEventBus.PublishAsync###{orderId}****HandleAsync{command.OrderId}";
    }

    /// <summary>
    ///     工作单元事件
    /// </summary>
    /// <param name="uowMgr"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("uow-event")]
    public async Task<string> UnitOfWorkEvent([FromServices] IUnitOfWorkManager uowMgr, CancellationToken cancellationToken)
    {
        var uow = await uowMgr.GetUnitOfWorkAsync(null, true, true, cancellationToken);

        var repo1 = uow.GetRepository<TestNewsInfo, int>();
        await repo1.PagedAsync(1, 20, cancellationToken: cancellationToken);

        uow.AddEventToBuffer(new PayedOrderCommand(SnowflakeIdUtility.Default().NextId()));

        // throw new Exception("自定义异常");

        await uow.CommitAsync(cancellationToken);

        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }
}