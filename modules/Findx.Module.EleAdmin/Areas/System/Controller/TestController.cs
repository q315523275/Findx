using System;
using System.ComponentModel;
using System.Reflection;
using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Findx.Data;
using Findx.AspNetCore.Mvc.Filters;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
    [Area("system")]
    [Route("api/[area]/test")]
    [Description("系统-测试")]
    public class TestController : AreaApiControllerBase
    {
        [HttpGet]
        [AuditOperation]
        [UnitOfWork]
        public async Task<CommonResult> Test([FromServices] IUnitOfWorkManager uowManager)
        {
            var uow = uowManager.GetEntityUnitOfWork<TestNewsInfo>(false);
            
            var repo1 = uow.GetRepository<TestNewsInfo>();
            var repo2 = uow.GetRepository<TestUserInfo>();
            
            // uow.BeginOrUseTransaction();

            // try
            // {
                var a1 = await repo1.PagedAsync(1, 20, orderParameters: new OrderByParameter<TestNewsInfo> { Expression = x => x.Id, SortDirection = ListSortDirection.Descending });
                var b2 = await repo2.PagedAsync(1, 20);

                var a = await repo1.SelectAsync();
                var b = await repo2.SelectAsync();

                var x = await repo1.DeleteAsync();
                var y = await repo2.DeleteAsync();

                throw new Exception("123");
            //
            //     uow.Commit();
            // }
            // catch
            // {
            //     uow.Rollback();
            // }

            return CommonResult.Success(new { Name = "测试", Date = DateTime.Now });
        }


        public class TestNewsInfo: IEntity
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
        }

        public class TestUserInfo : ISoftDeletable, IEntity
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public DateTime? DeletionTime { get; set; }
            public bool IsDeleted { get; set; }
        }
        
        public class MyClass: DispatchProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                throw new NotImplementedException();
            }
        }
    }
}

