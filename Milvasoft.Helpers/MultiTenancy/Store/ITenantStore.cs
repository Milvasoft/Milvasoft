﻿using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Store
{
    public interface ITenantStore<TTenant, TKey> : IMilvaTenantBase where TKey :  IEquatable<TKey>
    {
        Task<TTenant> GetTenantAsync(TKey identifier);
    }
}
