using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Extensions.ConfigurationServer.Dtos;
using Findx.Extensions.ConfigurationServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.ConfigurationServer.Controller;

/// <summary>
///     配置服务-应用
/// </summary>
[Area("findx")]
[Route("api/config/app")]
[Authorize]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-应用"), Description("配置服务-应用")]
public class AppController: CrudControllerBase<AppInfo, AppInfo, CreateAppDto, UpdateAppDto, QueryAppDto, long, long>;