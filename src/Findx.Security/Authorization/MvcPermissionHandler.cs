using Findx.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    public class MvcPermissionHandler : PermissionHandlerBase
    {
        private readonly IOptions<AuthorizationOptions> _options;
        private readonly IPermissionStore _store;

        public MvcPermissionHandler(IServiceProvider _provider, IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider, IOptions<AuthorizationOptions> options, IEnumerable<IPermissionStore> stores)
        {
            base.ServiceProvider = _provider;
            base.ActionDescriptorCollectionProvider = _actionDescriptorCollectionProvider;

            _options = options;
            _store = stores.FirstOrDefault(it => it.PermissionStoreType == options.Value.PermissionStoreType);
            Check.NotNull(_store, nameof(_store));
        }

        protected override Task<List<Permission>> GetFromStoreAsync()
        {
            return _store.GetFromStoreAsync();
        }

        protected override Task SyncToStoreAsync(List<Permission> permissions)
        {
            return _store.SyncToStoreAsync(permissions);
        }
    }
}
