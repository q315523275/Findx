using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Module.Admin.Const;
using Findx.Security;
using Findx.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Findx.Module.Admin.Sys.Filters;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统组织机构
    /// </summary>
    [DataScope]
    [Area("api/sys")]
    [Route("[area]/sysOrg")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysOrgController : CrudControllerBase<SysOrgInfo, SysOrgInfo, SysOrgRequest, SysOrgRequest, SysOrgQuery, long, long>
    {
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="currentUser"></param>
        public SysOrgController(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        /// <summary>
        /// 构建数据范围
        /// </summary>
        /// <returns></returns>
        private List<long> CreateDataScopes()
        {
            var dataScopes = _currentUser.DataScope();
            var dataScopes2 = new List<long>();
            // 此处获取所有的上级节点，放入set，用于构造完整树
            var repo = GetRepository<SysOrgInfo>();
            foreach (var item in dataScopes)
            {
                var model = repo.Get(item);
                string pids = model.Pids;
                string pidsWithRightSymbol = pids.Replace(SymbolConst.LEFT_SQUARE_BRACKETS, "");
                string pidsNormal = pidsWithRightSymbol.Replace(SymbolConst.RIGHT_SQUARE_BRACKETS, "");
                string[] pidsNormalArr = pidsNormal.Split(SymbolConst.COMMA);
                foreach (string pid in pidsNormalArr)
                {
                    if (!pid.IsNullOrWhiteSpace())
                        dataScopes2.Add(pid.To<long>());
                }
            }
            dataScopes.AddRange(dataScopes2);
            return dataScopes;
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysOrgInfo> CreatePageWhereExpression(SysOrgQuery request)
        {
            var dataScopes = new List<long>();
            if (!_currentUser.IsSuperAdmin())
            {
                dataScopes = CreateDataScopes();
            }
            return ExpressionBuilder.Create<SysOrgInfo>().AndIF(!_currentUser.IsSuperAdmin(), x => dataScopes.Contains(x.Id))
                                                         .AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                         .AndIF(!request.Pid.IsNullOrWhiteSpace(), x => x.Pids.Contains(request.Pid) || x.Id == request.Pid.To<long>());
        }

        /// <summary>
        /// 构建排序规则
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysOrgInfo>> CreatePageOrderExpression(SysOrgQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysOrgInfo>>();
            if (typeof(SysOrgInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysOrgInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysOrgInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }

        /// <summary>
        /// 树形数据查询
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        [HttpGet("tree")]
        public CommonResult Tree([FromServices] IFreeSql fsql, [FromServices] ICurrentUser currentUser)
        {
            var userId = currentUser.UserId.To<long>();
            var dataScopes = new List<long>();
            if (!currentUser.IsSuperAdmin())
            {
                dataScopes = CreateDataScopes();
            }

            var orgList = fsql.Select<SysOrgInfo>()
                              .WhereIf(!currentUser.IsSuperAdmin(), x => dataScopes.Contains(x.Id))
                              .Where(x => x.Status == 0).OrderBy(x => x.Sort)
                              .ToList(x => new SysOrgTreeNode
                              {
                                  Id = x.Id,
                                  ParentId = x.Pid,
                                  Title = x.Name,
                                  Value = x.Id.ToString(),
                                  Weight = x.Sort
                              });

            return CommonResult.Success(new TreeBuilder<SysOrgTreeNode, long>().Build(orgList, 0));
        }

        /// <summary>
        /// 创建Pids格式
        /// 如果pid是0顶级节点，pids就是 [0];
        /// 如果pid不是顶级节点，pids就是 pid菜单的 pids + [pid] + ,
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private async Task<string> CreateNewPids(long pid)
        {
            if (pid == 0L)
            {
                return "[0],";
            }
            else
            {
                var pmenu = await GetRepository<SysOrgInfo>().FirstAsync(u => u.Id == pid);
                return pmenu.Pids + "[" + pid + "],";
            }
        }

        /// <summary>
        /// 新增前操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="FindxException"></exception>
        protected override async Task AddBeforeAsync(SysOrgInfo model)
        {
            if (!_currentUser.IsSuperAdmin())
            {
                // 如果新增的机构父id不是0，则进行数据权限校验
                if (model.Pid != 0L)
                {
                    List<long> dataScope = _currentUser.DataScope();
                    // 数据范围为空
                    if (dataScope.IsEmpty())
                    {
                        throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                    }
                    else if (!dataScope.Contains(model.Pid))
                    {
                        // 所添加的组织机构的父机构不在自己的数据范围内
                        throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                    }
                }
                else
                {
                    // 如果新增的机构父id是0，则根本没权限，只有超级管理员能添加父id为0的节点
                    throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                }
            }

            var repo = GetRepository<SysOrgInfo>();
            var isExist = await repo.ExistAsync(u => u.Name == model.Name || u.Code == model.Code);
            if (isExist)
                throw new FindxException("D2002", "已有相同组织机构,编码或名称相同");

            model.Pids = await CreateNewPids(model.Pid);
            model.Status = CommonStatusEnum.ENABLE.CastTo<int>();
        }

        /// <summary>
        /// 编辑前操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="FindxException"></exception>
        protected override async Task EditBeforeAsync(SysOrgInfo model)
        {
            if (!_currentUser.IsSuperAdmin())
            {
                List<long> dataScope = _currentUser.DataScope();
                // 数据范围为空
                if (dataScope.IsEmpty())
                {
                    throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                }
                // 数据范围中不包含本公司
                else if (!dataScope.Contains(model.Id))
                {
                    throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                }
            }
            var repo = GetRepository<SysOrgInfo>();
            var old = await repo.FirstAsync(u => u.Name == model.Name || u.Code == model.Code);
            if (old != null && old.Id != model.Id)
                throw new FindxException("D2002", "已有相同组织机构,编码或名称相同");

            model.Pids = await CreateNewPids(model.Pid);
        }

        /// <summary>
        /// 删除前操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        protected override async Task DeleteBeforeAsync(List<DeleteParam<long>> req)
        {
            var repo = GetRepository<SysEmpInfo>();
            var repo2 = GetRepository<SysEmpExtOrgPosInfo>();
            List<long> dataScope = _currentUser.DataScope();
            foreach (var item in req)
            {
                if (!_currentUser.IsSuperAdmin())
                {
                    // 数据范围为空
                    if (dataScope.IsEmpty())
                    {
                        throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                    }
                    else if (!dataScope.Contains(item.Id))
                    {
                        // 所操作的数据不在自己的数据范围内
                        throw new FindxException("D2003", "没有权限操作该数据，请联系管理员");
                    }
                }
                // 该机构下有员工，则不能删
                bool hasOrgEmp = await repo.ExistAsync(x => x.OrgId == item.Id);
                if (hasOrgEmp)
                {
                    throw new FindxException("D2004", "该机构或子机构下有员工，无法删除");
                }
                // 该附属机构下若有员工，则不能删除
                bool hasExtOrgEmp = await repo2.ExistAsync(x => x.OrgId == item.Id);
                if (hasExtOrgEmp)
                {
                    throw new FindxException("D2004", "该机构或子机构下有员工，无法删除");
                }
            }
        }
    }
}
