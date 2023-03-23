using Microsoft.AspNetCore.Mvc;

namespace Findx.Swagger;

public class ApiDescriptionSettingsAttribute: ApiExplorerSettingsAttribute
{
    /// <summary>
    /// 标签
    /// </summary>
    public string Tag { set; get; }
}