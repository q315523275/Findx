using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Linq;
using Findx.Mapping;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Security;
using Findx.Security.Authorization;
using Findx.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysMenu")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysMenuController : CrudControllerBase<SysMenuInfo, SysMenuOutput, SysMenuInfo, SysMenuInfo, SysMenuQuery, long, long>
    {
        /// <summary>
        /// 构建排序
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysMenuInfo> CreatePageWhereExpression(SysMenuQuery request)
        {
            return ExpressionBuilder.Create<SysMenuInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                          .AndIF(!request.Application.IsNullOrWhiteSpace(), x => x.Application == request.Application)
                                                          .And(x => x.Status == CommonStatusEnum.ENABLE.To<int>());
        }

        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysMenuInfo>> CreatePageOrderExpression(SysMenuQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysMenuInfo>>();
            if (typeof(SysMenuInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysMenuInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysMenuInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }

        /// <summary>
        /// 切换应用菜单
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fsql"></param>
        /// <returns></returns>
        [HttpPost("change")]
        public CommonResult Change([FromBody] ChangeAppMenuInput input, [FromServices] IFreeSql fsql, [FromServices] ICurrentUser user)
        {
            var userId = user.UserId.To<long>();

            var roleIdList = fsql.Select<SysRoleInfo, SysUserRoleInfo>().InnerJoin((a, b) => a.Id == b.RoleId && b.UserId == userId).Where((a, b) => a.Status == 0)
                            .ToList((a, b) => a.Id);

            var menuList = fsql.Select<SysMenuInfo, SysRoleMenuInfo>().LeftJoin((a, b) => a.Id == b.MenuId)
                               .WhereIf(!user.IsSuperAdmin(), (a, b) => roleIdList.Contains(b.RoleId))
                               //.WhereIf(user.IsSuperAdmin(), (a, b) => a.Weight != 2) // 超级管理不碰业务
                               .Where((a, b) => a.Application == input.Application && a.Type != 2 && a.Status == 0)
                               .OrderBy((a, b) => a.Sort)
                               .ToList((a, b) => new LoginUserMenuDTO
                               {
                                   OpenType = a.OpenType,
                                   Id = a.Id,
                                   Pid = a.Pid,
                                   Name = a.Code,
                                   Component = a.Component,
                                   Redirect = a.Redirect,
                                   Path = a.Router,
                                   Hidden = a.Visible == "N",
                                   Meta = new LoginUserMenuMetaDTO
                                   {
                                       Title = a.Name,
                                       Icon = a.Icon,
                                       Show = a.Visible == "Y",
                                       Link = a.Link
                                   }
                               }).DistinctBy2(x => x.Id);

            foreach (var item in menuList)
            {
                if (MenuOpenTypeEnum.OUTER.To<int>() == item.OpenType)
                {
                    item.Meta.Target = "_blank";
                    item.Path = item.Meta.Link;
                    item.Redirect = item.Meta.Link;
                }
            }

            return CommonResult.Success(menuList);

        }

        /// <summary>
        /// 树形数据列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public override async Task<CommonResult> ListAsync([FromQuery] SysMenuQuery request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<SysMenuInfo>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var list = await repo.SelectAsync<SysMenuOutput>(whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());

            return CommonResult.Success(new TreeBuilder<SysMenuOutput, long>().Build(list, 0));
        }

        /// <summary>
        /// 获取系统菜单树，用于给角色授权时选择
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("treeForGrant")]
        public async Task<dynamic> TreeForGrant([FromQuery] TreeForGrantRequest req, [FromServices] IFreeSql fsql, [FromServices] ICurrentUser currentUser, [FromServices] IMapper mapper)
        {
            // 当前登陆用户编号
            var userId = currentUser.UserId.To<long>();

            var superAdmin = currentUser.IsSuperAdmin();

            var menuIdList = new List<long>();
            if (!superAdmin)
            {
                var roleIdList = fsql.Select<SysRoleInfo, SysUserRoleInfo>().InnerJoin((a, b) => a.Id == b.RoleId && b.UserId == userId).Where((a, b) => a.Status == 0)
                                .ToList((a, b) => a.Id);
                menuIdList = fsql.Select<SysRoleMenuInfo>().Where(u => roleIdList.Contains(u.RoleId)).ToList(x => x.MenuId);
            }

            var menus = await fsql.Select<SysMenuInfo>()
                                  .WhereIf(!req.Application.IsNullOrWhiteSpace(), u => u.Application == req.Application)
                                  .Where(u => u.Status == 0)
                                  .WhereIf(menuIdList.Count() > 0, u => menuIdList.Contains(u.Id))
                                  .OrderBy(u => u.Sort)
                                  .ToListAsync(u => new SysMenuTreeOutput
                                  {
                                      Id = u.Id,
                                      ParentId = u.Pid,
                                      IntValue = u.Id,
                                      Title = u.Name,
                                      Weight = u.Weight ?? 0
                                  });
            return CommonResult.Success(new TreeBuilder<SysMenuTreeOutput, long>().Build(menus, 0));
        }

        /// <summary>
        /// 获取系统菜单树，用于新增、编辑时选择上级节点
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("tree")]
        public async Task<dynamic> GetMenuTree([FromQuery] SysMenuQuery req, [FromServices] IRepository<SysMenuInfo> repo)
        {
            var typeValues = new int[] { 0, 1 };
            var menus = await repo.SelectAsync(x => x.Application == req.Application && x.Status == 0 && typeValues.Contains(x.Type)
                                        , selectExpression: x => new SysMenuTreeOutput
                                        {
                                            Id = x.Id,
                                            ParentId = x.Pid,
                                            IntValue = x.Id,
                                            Title = x.Name,
                                            Weight = x.Weight.Value
                                        }
                                        , orderParameters: CreatePageOrderExpression(req).ToArray());
            return CommonResult.Success(new TreeBuilder<SysMenuTreeOutput, long>().Build(menus, 0));
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
                var pmenu = await GetRepository<SysMenuInfo>().FirstAsync(u => u.Id == pid);
                return pmenu.Pids + "[" + pid + "],";
            }
        }

        /// <summary>
        /// 增加和编辑时检查参数
        /// </summary>
        /// <param name="input"></param>
        private static void CheckMenuParam(SysMenuInfo input)
        {
            var type = input.Type;
            var router = input.Router;
            var permission = input.Permission;
            var openType = input.OpenType;

            if (type.Equals((int)MenuTypeEnum.DIR))
            {
                if (string.IsNullOrEmpty(router))
                    throw new FindxException("D4001", "路由地址为空");
            }
            else if (type.Equals((int)MenuTypeEnum.MENU))
            {
                if (string.IsNullOrEmpty(router))
                    throw new FindxException("D4001", "路由地址为空");
                if (string.IsNullOrEmpty(openType.ToString()))
                    throw new FindxException("D4002", "打开方式为空");
            }
            else if (type.Equals((int)MenuTypeEnum.BTN))
            {
                if (string.IsNullOrEmpty(permission))
                    throw new FindxException("D4003", "权限标识格式为空");
                if (!permission.Contains(":"))
                    throw new FindxException("D4004", "权限标识格式错误");
            }
        }

        /// <summary>
        /// 新增之前操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected override async Task AddBeforeAsync(SysMenuInfo model)
        {
            var repo = GetRepository<SysMenuInfo>();
            var isExist = await repo.ExistAsync(u => u.Code == model.Code);
            if (isExist)
                throw new FindxException("D4000", "菜单已存在");
            model.Pids = await CreateNewPids(model.Pid);
            model.Status = CommonStatusEnum.ENABLE.CastTo<int>();
        }

        /// <summary>
        /// 编辑之前操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="FindxException"></exception>
        protected override async Task EditBeforeAsync(SysMenuInfo model)
        {
            // Pid和Id不能一致，一致会导致无限递归
            if (model.Id == model.Pid)
                throw new FindxException("D4006", "父级菜单不能为当前节点，请重新选择父级菜单");

            var repo = GetRepository<SysMenuInfo>();
            var isExist = await repo.ExistAsync(u => u.Code == model.Code && u.Id != model.Id);
            if (isExist)
                throw new FindxException("D4000", "菜单已存在");

            // 校验参数
            CheckMenuParam(model);
            // 如果是编辑，父id不能为自己的子节点
            var childIdList = await repo.SelectAsync(u => u.Pids.Contains(model.Id.ToString()), selectExpression: u => u.Id);
            if (childIdList.Contains(model.Pid))
                throw new FindxException("D4006", "父级菜单不能为当前节点，请重新选择父级菜单");

            var oldMenu = await repo.FirstAsync(u => u.Id == model.Id);

            // 生成新的pids
            var newPids = await CreateNewPids(model.Pid);
            // 更新当前菜单
            model.Pids = newPids;
            // 是否更新子应用的标识
            var updateSubAppsFlag = false;
            // 是否更新子节点的pids的标识
            var updateSubPidsFlag = false;

            // 如果应用有变化
            if (model.Application != oldMenu.Application)
            {
                // 父节点不是根节点不能移动应用
                if (oldMenu.Pid != 0L)
                    throw new FindxException("D4007", "不能移动根节点");
                updateSubAppsFlag = true;
            }
            // 父节点有变化
            if (model.Pid != oldMenu.Pid)
                updateSubPidsFlag = true;

            // 开始更新所有子节点的配置
            if (updateSubAppsFlag || updateSubPidsFlag)
            {
                // 查找所有叶子节点，包含子节点的子节点
                var menuList = await repo.SelectAsync(u => u.Pids.Contains($"%{oldMenu.Id}%"));
                // 更新所有子节点的应用为当前菜单的应用
                if (menuList.Count > 0)
                {
                    // 更新所有子节点的application
                    if (updateSubAppsFlag)
                    {
                        menuList.ForEach(u =>
                        {
                            u.Application = model.Application;
                        });
                    }

                    // 更新所有子节点的pids
                    if (updateSubPidsFlag)
                    {
                        menuList.ForEach(u =>
                        {
                            // 子节点pids组成 = 当前菜单新pids + 当前菜单id + 子节点自己的pids后缀
                            var oldParentCodesPrefix = oldMenu.Pids + "[" + oldMenu.Id + "],";
                            var oldParentCodesSuffix = u.Pids[oldParentCodesPrefix.Length..];
                            var menuParentCodes = newPids + "[" + oldMenu.Id + "]," + oldParentCodesSuffix;
                            u.Pids = menuParentCodes;
                        });
                    }

                    repo.Update(menuList);
                }
            }
        }
    }
}
