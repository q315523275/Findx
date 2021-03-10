using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Abstractions
{
    public interface IDiscoveryClient : IServiceInstanceProvider
    {
        /// <summary>
        /// 查询组下所有服务有效实例
        /// </summary>
        /// <param name="group"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IList<IServiceInstance>> GetAllInstancesAsync(string group = null, CancellationToken cancellationToken = default);
    }
}
