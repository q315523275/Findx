using Findx.Data;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class RepoController: Controller
    {
        [HttpGet("/repo/test")]
        public async Task<string> RepoTest([FromServices] IRepository<TestNewsInfo> repo1, [FromServices] IRepository<TestUserInfo> repo2)
        {
            await repo1.SelectAsync();
            await repo2.SelectAsync();

            await repo1.DeleteAsync();
            await repo2.DeleteAsync();

            return DateTime.Now.ToString();
        }
    }
}
