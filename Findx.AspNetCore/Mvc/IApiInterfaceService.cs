using Findx.AspNetCore.Mvc.Models;
using System.Collections.Generic;

namespace Findx.AspNetCore.Mvc
{
    public interface IApiInterfaceService
    {
        /// <summary>
        /// 获取所有控制器。不包含抽象的类
        /// </summary>
        IEnumerable<ControllerDescriptor> GetAllController();

        /// <summary>
        /// 获取所有操作
        /// </summary>
        IEnumerable<ActionDescriptor> GetAllAction();
    }
}
