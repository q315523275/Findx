﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Security.Authorization.Store
{
    public class DatabasePermissionStore : IPermissionStore
    {
        public PermissionStoreType PermissionStoreType => throw new NotImplementedException();

        public Task<List<Permission>> GetFromStoreAsync()
        {
            throw new NotImplementedException();
        }

        public Task SyncToStoreAsync(List<Permission> permissions)
        {
            throw new NotImplementedException();
        }
    }
}
