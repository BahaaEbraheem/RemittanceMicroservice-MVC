﻿using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using RemittanceManagement;

namespace CurrencyManagment;

[DependsOn(
    typeof(CurrencyManagmentDomainModule),
    typeof(CurrencyManagmentApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(RemittanceManagementHttpApiClientModule)



    )]
public class CurrencyManagmentApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<CurrencyManagmentApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<CurrencyManagmentApplicationModule>(validate: true);
        });
    }
}
