using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.LoginRecord;
using Findx.Module.EleAdminPlus.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     登录日志
/// </summary>
[Area("system")]
[Route("api/[area]/login-record")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-登录日志"), Description("系统-登录日志")]
[DisableAuditing]
public class LoginRecordController : QueryControllerBase<SysLoginRecordInfo, LoginRecordDto, LoginRecordPageQueryDto, long>;
