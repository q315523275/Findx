using System;

namespace Findx.Component.DistributedConfigurationCenter.Dtos;

/// <summary>
/// 更新App Dto
/// </summary>
public class UpdateAppDto: CreateAppDto
{
    /// <summary>
    ///     主键id
    /// </summary>
    public Guid Id { get; set; }
}