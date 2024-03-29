﻿using CustomerManagement.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace CustomerManagement;

/* Domain tests are configured to use the EF Core provider.
 * You can switch to MongoDB, however your domain tests should be
 * database independent anyway.
 */
[DependsOn(
    typeof(CustomerManagementEntityFrameworkCoreTestModule)
    )]
public class CustomerManagementDomainTestModule : AbpModule
{

}
