using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     自动生成crud服务
/// </summary>
[Description("自动生成crud服务"), Tags("自动生成crud服务")]
[Route("api/crud")]
public class CrudController: QueryControllerBase<TestNewsInfo, TestNewsInfo, QueryNewsDto, int>;