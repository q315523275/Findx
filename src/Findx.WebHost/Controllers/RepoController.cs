using Findx.Data;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class RepoController : Controller
    {
        [HttpGet("/repo/test")]
        public async Task<string> RepoTest([FromServices] IRepository<TestNewsInfo> repo1, [FromServices] IRepository<TestUserInfo> repo2)
        {
            var uow = repo1.GetUnitOfWork();

            uow.BeginOrUseTransaction();

            try
            {

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
