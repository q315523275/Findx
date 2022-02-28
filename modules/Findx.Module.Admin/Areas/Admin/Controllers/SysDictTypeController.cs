using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("api/admin")]
    [Route("[area]/sysDictType")]
    public class SysDictTypeController : CrudControllerBase<SysDictTypeInfo, SysDictTypeInfo, SysDictTypeRequest, SysDictTypeRequest, SysQuery, long, long>
    {

        /// <summary>
        /// 查询字典值
        /// </summary>
        /// <param name="code"></param>
        /// <param name="repo"></param>
        /// <param name="repo_data"></param>
        /// <returns></returns>
        [HttpGet("dropDown")]
        public CommonResult DropDown([Required] string code, [FromServices] IRepository<SysDictTypeInfo> repo, [FromServices] IRepository<SysDictDataInfo> repo_data)
        {
            var dictType = repo.First(u => u.Code == code);
            if (dictType == null)
                return CommonResult.Fail("500", "没有字典配置数据");

            var list = repo_data.Select(it => it.TypeId == dictType.Id && it.Status == 0, it => new SysDictDataInfo { Code = it.Code, Value = it.Value, Sort = it.Sort }).OrderBy(it => it.Sort);

            return CommonResult.Success(list);
        }

        protected override List<OrderByParameter<SysDictTypeInfo>> CreatePageOrderExpression(SysQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysDictTypeInfo>>();
            if (typeof(SysDictTypeInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysDictTypeInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysDictTypeInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }

        [HttpGet("tree")]
        public CommonResult Tree()
        {
            var repo = GetRepository<SysDictTypeInfo>();
            var repo_data = GetRepository<SysDictDataInfo>();

            var list = repo.Select();
            var data = repo_data.Select();

            return CommonResult.Success(list.Select(x => new
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Children = data.Where(it => it.TypeId == x.Id).Select(c => new
                {
                    Id = c.Id,
                    Pid = c.TypeId,
                    Code = c.Code,
                    Name = c.Value
                })
            }));
        }
    }
}
