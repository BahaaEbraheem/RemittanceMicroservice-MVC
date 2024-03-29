﻿using AmlManagement.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace AmlManagement
{
    /* Domain tests are configured to use the EF Core provider.
     * You can switch to MongoDB, however your domain tests should be
     * database independent anyway.
     */
    [DependsOn(
        typeof(AmlManagementEntityFrameworkCoreTestModule)
        )]
    public class AmlManagementDomainTestModule : AbpModule
    {
        
    }
}
