using System;
using System.Threading.Tasks;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.WebHost.Controllers;

/// <summary>
/// 数据仓储
/// </summary>
public class RepoController : Controller
{
    /// <summary>
    /// repo
    /// </summary>
    /// <param name="uowManager"></param>
    /// <param name="keyGenerator"></param>
    /// <returns></returns>
    [HttpGet("/repo/test")]
    public async Task<Guid> RepoTest([FromServices] IUnitOfWorkManager uowManager, [FromServices] IKeyGenerator<Guid> keyGenerator)
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
    /// repo
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <returns></returns>
    [HttpGet("/repo/async")]
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
    
    
}