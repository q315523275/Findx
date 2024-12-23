using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.OperationRecord;
using Findx.Module.EleAdminPlus.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     操作日志
/// </summary>
[Area("system")]
[Route("api/[area]/operation-record")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-操作日志"), Description("系统-操作日志")]
[DisableAuditing]
public class OperationRecordController : QueryControllerBase<SysOperationRecordInfo, OperationRecordDto, OperationRecordPageQueryDto, long>;
