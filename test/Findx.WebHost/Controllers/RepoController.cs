using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.WebHost.Controllers;

/// <summary>
///     数据仓储
/// </summary>
[Route("api/repo")]
[Description("数据仓储"), Tags("数据仓储")]
public class RepoController : ApiControllerBase
{
    /// <summary>
    ///     异步仓储工作单元
    /// </summary>
    /// <param name="uowManager"></param>
    /// <param name="keyGenerator"></param>
    /// <returns></returns>
    [HttpGet("test")]
    public async Task<Guid> Repo([FromServices] IUnitOfWorkManager uowManager, [FromServices] IKeyGenerator<Guid> keyGenerator)
    {
        await using (var uow = await uowManager.GetConnUnitOfWorkAsync(true, true))
        {
            var repo1 = uow.GetRepository<TestNewsInfo, int>();
            var repo2 = uow.GetRepository<TestUserInfo, int>();
            var a1 = await repo1.PagedAsync(1, 20);
            var b2 = await repo2.PagedAsync(1, 20);

            var a = await repo1.SelectAsync();
            var b = await repo2.SelectAsync();

            var x = await repo1.DeleteAsync();
            var y = await repo2.DeleteAsync();

            // a1.Rows.First().GetProperty<string>("title");
            await uow.CommitAsync();
        }
        return keyGenerator.Create();
    }
    
    /// <summary>
    ///     异步线程仓储工作单元
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <returns></returns>
    [HttpGet("async")]
    public async Task<Guid> RepoAsync([FromServices] IKeyGenerator<Guid> keyGenerator)
    {
        await using (var scope = ServiceLocator.Instance.CreateAsyncScope())
        {
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            await using (var uow = await unitOfWorkManager.GetConnUnitOfWorkAsync(true, true))
            {
                var repo1 = uow.GetRepository<TestNewsInfo, int>();
                var repo2 = uow.GetRepository<TestUserInfo, int>();
                var a1 = await repo1.PagedAsync(1, 20);
                var b2 = await repo2.PagedAsync(1, 20);

                var a = await repo1.SelectAsync();
                var b = await repo2.SelectAsync();

                var x = await repo1.DeleteAsync();
                var y = await repo2.DeleteAsync();

                // a1.Rows.First().GetProperty<string>("title");
                await uow.CommitAsync();
            }
        }
        return keyGenerator.Create();
    }
    
    
    /// <summary>
    ///     异步线程仓储工作单元+审计
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <returns></returns>
    [HttpGet("auditing")]
    [AuditOperation]
    public async Task<Guid> UpdateAsync([FromServices] IKeyGenerator<Guid> keyGenerator)
    {
        await using (var scope = ServiceLocator.Instance.CreateAsyncScope())
        {
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            await using (var uow = await unitOfWorkManager.GetConnUnitOfWorkAsync(true, true))
            {
                var repo = uow.GetRepository<TestNewsInfo, int>();

                var id = Utilities.RandomUtility.RandomInt();
                var model = new TestNewsInfo { Id = id };
                Console.WriteLine($"实体初始化:{model.GetHashCode()}");
                
                model.SetProperty("key1", "test");
                await repo.InsertAsync(model);
                
                model.SetProperty("key1", "test2222");
                await repo.UpdateAsync(model, updateColumns: x => new { x.ExtraProperties });
                
                await repo.DeleteAsync();
                await uow.CommitAsync();
            }
        }
        return keyGenerator.Create();
    }
    
    
}