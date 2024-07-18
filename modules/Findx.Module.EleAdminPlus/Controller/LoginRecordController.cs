using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Module.EleAdminPlus.Dtos.LoginRecord;
using Findx.Module.EleAdminPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     登录日志
/// </summary>
[Area("system")]
[Route("api/[area]/login-record")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-登录日志"), Description("系统-登录日志")]
public class LoginRecordController : QueryControllerBase<SysLoginRecordInfo, LoginRecordDto, QueryLoginRecordDto, long>;