using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// View视图操作
    /// </summary>
    public interface IRazorViewRender
    {
        /// <summary>
        /// 视图数据绑定并返回
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
