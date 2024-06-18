using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.ConfigurationServer.Dtos;
using Findx.Extensions.ConfigurationServer.Models;
using Findx.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.ConfigurationServer.Controller;

/// <summary>
/// 配置服务App
/// </summary>
[Area("findx")]
[Route("api/config/app")]
[Authorize]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-App"), Description("配置服务App")]
public class AppController: CrudControllerBase<AppInfo, AppInfo, CreateAppDto, UpdateAppDto, QueryAppDto, long, long>;