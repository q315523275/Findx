using Findx.Data;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc.Filters;

namespace Findx.WebHost.Controllers
{
    public class RepoController : Controller
    {
        [HttpGet("/repo/test")]
        public async Task<string> RepoTest([FromServices] IRepository<TestNewsInfo> repo1, [FromServices] IRepository<TestUserInfo> repo2, [FromServices] IUnitOfWorkManager uowManager)
        {
            var uow =  uowManager.GetConnUnitOfWork(true);

            uow.BeginOrUseTransaction();

            try
            {
                var a1 = await repo1.PagedAsync(1, 20, orderParameters: new List<OrderByParameter<TestNewsInfo>>{ new(){  Expression = x => x.Id, SortDirection = System.ComponentModel.ListSortDirection.Descending } });
                var b2 = await repo2.PagedAsync(1, 20);

                var a = await repo1.SelectAsync();
                var b = await repo2.SelectAsync();

                var x = await repo1.DeleteAsync();
                var y = await repo2.DeleteAsync().WaitAsync(TimeSpan.FromSeconds(10));

                throw new Exception("123");

                uow.Commit();
            }
            catch
            {
                uow.Rollback();
            }

            return DateTime.Now.ToString();
        }
    }
}
