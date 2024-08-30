using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     自动生成crud服务
/// </summary>
[Description("Crud服务"), Tags("Crud服务")]
[Route("api/crud")]
public class CrudController : CrudControllerBase<TestNewsInfo, TestNewsInfo, QueryNewsDto, int, int>
{
    [Transactional]
    public override Task<CommonResult> DeleteAsync(List<int> request, CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync(request, cancellationToken);
    }
}