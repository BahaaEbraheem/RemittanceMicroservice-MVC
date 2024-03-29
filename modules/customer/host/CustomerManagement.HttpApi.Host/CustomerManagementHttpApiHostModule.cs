using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using CustomerManagement.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using MsDemo.Shared;
using Volo.Abp.Threading;
using Volo.Abp.Data;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.EventBus.RabbitMq;

namespace CustomerManagement;
[DependsOn(
    typeof(CustomerManagementApplicationModule),
    typeof(CustomerManagementEntityFrameworkCoreModule),
    typeof(CustomerManagementHttpApiModule),
    typeof(AbpAutofacModule),
        typeof(AbpAspNetCoreMvcModule),
        typeof(AbpEventBusRabbitMqModule),
        typeof(AbpEntityFrameworkCoreSqlServerModule),
        typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        typeof(AbpPermissionManagementEntityFrameworkCoreModule),
        typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpAspNetCoreMultiTenancyModule),
        typeof(AbpTenantManagementEntityFrameworkCoreModule)
    )]
public class CustomerManagementHttpApiHostModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {



        var configuration = context.Services.GetConfiguration();

        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MsDemoConsts.IsMultiTenancyEnabled;
        });

        context.Services.AddAuthentication("Bearer")
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.ApiName = configuration["AuthServer:ApiName"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
            });

        context.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Customer Management API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });

        //context.Services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = configuration["Redis:Configuration"];
        //});

        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabledForGetRequests = true;
            options.ApplicationName = "CustomerManagement";
        });

        //var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        //context.Services.AddDataProtection()
        //    .PersistKeysToStackExchangeRedis(redis, "MsDemo-DataProtection-Keys");

    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAbpClaimsMap();
        //app.UseCors();

        if (MsDemoConsts.IsMultiTenancyEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAbpRequestLocalization(); //TODO: localization?
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Management API");
        });
        app.UseAuditing();
        app.UseConfiguredEndpoints();

        //TODO: Problem on a clustered environment
        AsyncHelper.RunSync(async () =>
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            }
        });
    }
}























//        var hostingEnvironment = context.Services.GetHostingEnvironment();
//        var configuration = context.Services.GetConfiguration();

//        Configure<AbpDbContextOptions>(options =>
//        {
//            options.UseSqlServer();
//        });

//        Configure<AbpMultiTenancyOptions>(options =>
//        {
//            options.IsEnabled = MultiTenancyConsts.IsEnabled;
//        });

//        if (hostingEnvironment.IsDevelopment())
//        {
//            Configure<AbpVirtualFileSystemOptions>(options =>
//            {
//                options.FileSets.ReplaceEmbeddedByPhysical<CustomerManagementDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}CustomerManagement.Domain.Shared", Path.DirectorySeparatorChar)));
//                options.FileSets.ReplaceEmbeddedByPhysical<CustomerManagementDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}CustomerManagement.Domain", Path.DirectorySeparatorChar)));
//                options.FileSets.ReplaceEmbeddedByPhysical<CustomerManagementApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}CustomerManagement.Application.Contracts", Path.DirectorySeparatorChar)));
//                options.FileSets.ReplaceEmbeddedByPhysical<CustomerManagementApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}CustomerManagement.Application", Path.DirectorySeparatorChar)));
//            });
//        }

//        context.Services.AddAbpSwaggerGenWithOAuth(
//            configuration["AuthServer:Authority"],
//            new Dictionary<string, string>
//            {
//                {"CustomerManagement", "CustomerManagement API"}
//            },
//            options =>
//            {
//                options.SwaggerDoc("v1", new OpenApiInfo {Title = "CustomerManagement API", Version = "v1"});
//                options.DocInclusionPredicate((docName, description) => true);
//                options.CustomSchemaIds(type => type.FullName);
//            });

//        Configure<AbpLocalizationOptions>(options =>
//        {
//            options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
//            options.Languages.Add(new LanguageInfo("cs", "cs", "Čeština"));
//            options.Languages.Add(new LanguageInfo("en", "en", "English"));
//            options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (UK)"));
//            options.Languages.Add(new LanguageInfo("fi", "fi", "Finnish"));
//            options.Languages.Add(new LanguageInfo("fr", "fr", "Français"));
//            options.Languages.Add(new LanguageInfo("hi", "hi", "Hindi", "in"));
//            options.Languages.Add(new LanguageInfo("is", "is", "Icelandic", "is"));
//            options.Languages.Add(new LanguageInfo("it", "it", "Italiano", "it"));
//            options.Languages.Add(new LanguageInfo("hu", "hu", "Magyar"));
//            options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Português"));
//            options.Languages.Add(new LanguageInfo("ro-RO", "ro-RO", "Română"));
//            options.Languages.Add(new LanguageInfo("ru", "ru", "Русский"));
//            options.Languages.Add(new LanguageInfo("sk", "sk", "Slovak"));
//            options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe"));
//            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
//            options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "繁體中文"));
//            options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "Deutsch"));
//            options.Languages.Add(new LanguageInfo("es", "es", "Español"));
//            options.Languages.Add(new LanguageInfo("el", "el", "Ελληνικά"));
//        });

//        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//            .AddJwtBearer(options =>
//            {
//                options.Authority = configuration["AuthServer:Authority"];
//                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
//                options.Audience = "CustomerManagement";
//            });

//        Configure<AbpDistributedCacheOptions>(options =>
//        {
//            options.KeyPrefix = "CustomerManagement:";
//        });

//        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("CustomerManagement");
//        if (!hostingEnvironment.IsDevelopment())
//        {
//            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
//            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "CustomerManagement-Protection-Keys");
//        }

//        context.Services.AddCors(options =>
//        {
//            options.AddDefaultPolicy(builder =>
//            {
//                builder
//                    .WithOrigins(
//                        configuration["App:CorsOrigins"]
//                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
//                            .Select(o => o.RemovePostFix("/"))
//                            .ToArray()
//                    )
//                    .WithAbpExposedHeaders()
//                    .SetIsOriginAllowedToAllowWildcardSubdomains()
//                    .AllowAnyHeader()
//                    .AllowAnyMethod()
//                    .AllowCredentials();
//            });
//        });
//    }

//    public override void OnApplicationInitialization(ApplicationInitializationContext context)
//    {
//        var app = context.GetApplicationBuilder();
//        var env = context.GetEnvironment();

//        if (env.IsDevelopment())
//        {
//            app.UseDeveloperExceptionPage();
//        }
//        else
//        {
//            app.UseHsts();
//        }

//        app.UseHttpsRedirection();
//        app.UseCorrelationId();
//        app.UseStaticFiles();
//        app.UseRouting();
//        app.UseCors();
//        app.UseAuthentication();
//        if (MultiTenancyConsts.IsEnabled)
//        {
//            app.UseMultiTenancy();
//        }
//        app.UseAbpRequestLocalization();
//        app.UseAuthorization();
//        app.UseSwagger();
//        app.UseAbpSwaggerUI(options =>
//        {
//            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API");

//            var configuration = context.GetConfiguration();
//            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
//            options.OAuthScopes("CustomerManagement");
//        });
//        app.UseAuditing();
//        app.UseAbpSerilogEnrichers();
//        app.UseConfiguredEndpoints();
//    }
//}
