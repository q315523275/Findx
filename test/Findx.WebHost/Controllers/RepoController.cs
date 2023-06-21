﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Findx.Data;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

public class RepoController : Controller
{
    [HttpGet("/repo/test")]
    public async Task<string> RepoTest([FromServices] IRepository<TestNewsInfo, int> repo1, [FromServices] IRepository<TestUserInfo, int> repo2, [FromServices] IUnitOfWorkManager uowManager)
    {
        var uow = await uowManager.GetConnUnitOfWorkAsync(true, true);

        try
        {
            var a1 = await repo1.PagedAsync(1, 20,
                orderParameters: new List<OrderByParameter<TestNewsInfo>>
                    { new() { Expression = x => x.Id, SortDirection = ListSortDirection.Descending } });
            var b2 = await repo2.PagedAsync(1, 20);

            var a = await repo1.SelectAsync();
            var b = await repo2.SelectAsync();

            var x = await repo1.DeleteAsync();
            var y = await repo2.DeleteAsync().WaitAsync(TimeSpan.FromSeconds(10));

            a1.Rows.First().GetProperty<string>("title");

            throw new Exception("123");

            uow.CommitAsync();
        }
        catch
        {
            uow.RollbackAsync();
        }

        return DateTime.Now.ToString();
    }
}