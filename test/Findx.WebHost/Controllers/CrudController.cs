using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Components;

namespace Findx.WebHost.Controllers;

/// <summary>
/// CrudController
/// </summary>
[Description("CrudController")]
[Route("crud")]
public class CrudController: QueryControllerBase<TestNewsInfo, TestNewsInfo, QueryNewsDto, int>;