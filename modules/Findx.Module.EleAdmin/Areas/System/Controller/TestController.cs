using System;
using System.ComponentModel;
using System.Reflection;
using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Findx.Data;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
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
                repo1.WithUnitOfWork(uow).Insert(new TestNewsInfo());
            
                var a1 = await repo1.PagedAsync(1, 20, orderParameters: new OrderByParameter<TestNewsInfo> { Expression = x => x.Id, SortDirection = ListSortDirection.Descending });
                var b2 = await repo2.PagedAsync(1, 20);

                var a = await repo1.SelectAsync();
                var b = await repo2.SelectAsync();

                var x = await repo1.DeleteAsync();
                var y = await repo2.DeleteAsync();

            //    throw new Exception("123");
            //
            //     uow.Commit();
            // }
            // catch
            // {
            //     uow.Rollback();
            // }
            
            var param = new
            {
                NeedUpDateFields = Array.Empty<string>(),
                NeedReturnFields = Array.Empty<string>(),
                IsDeleteEntry = true,
                SubSystemId = "",
                IsVerifyBaseDataField = false,
                IsEntryBatchFill = true,
                ValidateFlag = true,
                NumberSearch = true,
                IsAutoAdjustField = false,
                InterationFlags = "",
                IgnoreInterationFlag = "",
                Model = new
                {
                    FBillTypeID = new { FNumber = "G02" },
                    FDate = DateTime.Now, // 下单时间
                    FSaleOrgId = new { FNumber = "100" }, // 销售组织
                    FSaleDeptId = new { FNumber = "100" }, // 部门
                    FExchangeTypeId = new { FNumber = "HLTX01_SYS" }, // 汇率类型
                    FPayerCombo = "3", // 选择付款方
                    FINVOICETYPE = "1", // 发票类型
                    FSupplyOrgId = new { FNumber = "100" }, // 供应组织
                    F_QVFJ_Calc_GongSi = false, // 工时已算
                }
            };

            return CommonResult.Success(new { Name = "测试", Date = DateTime.Now, Param = param.ToJson() });
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

