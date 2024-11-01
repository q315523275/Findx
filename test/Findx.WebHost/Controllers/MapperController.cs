using System;
using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Mapping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     实体映射
/// </summary>
[Route("api/mapper")]
[Description("映射服务"), Tags("映射服务")]
public class MapperController: ApiControllerBase
{

    [HttpPost("test")]
    public CommonResult Test([FromBody] UserDto req)
    {
        var user = new User();
        user = req.MapTo(user);
        return CommonResult.Success(user);
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime? Birthday { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime? Birthday { get; set; }
}