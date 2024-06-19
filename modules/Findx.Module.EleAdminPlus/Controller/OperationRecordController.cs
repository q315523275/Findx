using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Module.EleAdminPlus.Dtos;
using Findx.Module.EleAdminPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     操作日志
/// </summary>
[Area("system")]
[Route("api/[area]/operation-record")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-操作日志"), Description("系统-操作日志")]
public class OperationRecordController : QueryControllerBase<SysOperationRecordInfo, SysOperationRecordInfo, QueryOperationRecordRequest, long>;