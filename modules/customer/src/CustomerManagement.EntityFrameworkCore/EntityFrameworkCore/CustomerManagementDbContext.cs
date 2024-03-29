﻿using CustomerManagement.Customers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace CustomerManagement.EntityFrameworkCore;

[ConnectionStringName(CustomerManagementDbProperties.ConnectionStringName)]
public class CustomerManagementDbContext : AbpDbContext<CustomerManagementDbContext>, ICustomerManagementDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */
    public DbSet<Customer> Customers { get; set; }

    public CustomerManagementDbContext(DbContextOptions<CustomerManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureCustomerManagement();
    }
}
