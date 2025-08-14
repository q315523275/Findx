using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.LoginRecord;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Net.IP;
using Findx.Setting;
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
public class LoginRecordController : QueryControllerBase<SysLoginRecordInfo, LoginRecordDto, LoginRecordPageQueryDto, long>
{
    private readonly IIpGeolocation _ipGeolocation;
    private readonly bool _enabledGeolocation;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="ipGeolocation"></param>
    /// <param name="settingProvider"></param>
    public LoginRecordController(IIpGeolocation ipGeolocation, ISettingProvider settingProvider)
    {
        _ipGeolocation = ipGeolocation;
        _enabledGeolocation = settingProvider.GetValue<bool>("Modules:EleAdminPlus:EnabledGeolocation");
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<PageResult<List<LoginRecordDto>>>> PageAsync(LoginRecordPageQueryDto req, CancellationToken cancellationToken = default)
    {
        var rs = await base.PageAsync(req, cancellationToken);
        GetIpAddress(rs.Data.Rows);
        return rs;
    }

    /// <summary>
    ///     列表查询
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<LoginRecordDto>>> ListAsync(LoginRecordPageQueryDto req, CancellationToken cancellationToken = default)
    {
        var rs = await base.ListAsync(req, cancellationToken);
        GetIpAddress(rs.Data);
        return rs;
    }

    /// <summary>
    ///     循环获取IP归属
    /// </summary>
    /// <param name="rows"></param>
    private void GetIpAddress(List<LoginRecordDto> rows)
    {
        if (!_enabledGeolocation) return;
        
        var ipList = rows.Select(x => x.Ip).Distinct();
        var dict = new Dictionary<string, string>();
        foreach (var item in ipList)
        {
            var rs = _ipGeolocation.GetLocationByIp(item);
            dict.Add(item, rs.IsInternalIp ? "内网" : $"{rs.Country}{rs.Province}{rs.City}{rs.Isp}");
        }
        foreach (var item in rows)
        {
            if (dict.TryGetValue(item.Ip, out var ipAddress))
            {
                item.IpAddress = ipAddress;
            }
        }
    }
}
