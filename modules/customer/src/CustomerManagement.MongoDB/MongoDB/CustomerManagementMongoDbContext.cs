﻿using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace CustomerManagement.MongoDB;

[ConnectionStringName(CustomerManagementDbProperties.ConnectionStringName)]
public class CustomerManagementMongoDbContext : AbpMongoDbContext, ICustomerManagementMongoDbContext
{
    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureCustomerManagement();
    }
}
