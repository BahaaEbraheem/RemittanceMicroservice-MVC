﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.IdentityServer.ApiResources;
using Volo.Abp.IdentityServer.ApiScopes;
using Volo.Abp.IdentityServer.Clients;
using Volo.Abp.IdentityServer.IdentityResources;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using ApiResource = Volo.Abp.IdentityServer.ApiResources.ApiResource;
using Client = Volo.Abp.IdentityServer.Clients.Client;

namespace AuthServer.Host
{
    public class AuthServerDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IApiResourceRepository _apiResourceRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IApiScopeRepository _apiScopeRepository;
        private readonly IIdentityResourceDataSeeder _identityResourceDataSeeder;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IPermissionDataSeeder _permissionDataSeeder;
        private readonly IRepository<IdentityRole, Guid> _roleRepository;

        public AuthServerDataSeeder(
            IRepository<IdentityRole, Guid> roleRepository,
            IClientRepository clientRepository,
            IApiResourceRepository apiResourceRepository,
            IIdentityResourceDataSeeder identityResourceDataSeeder,
            IGuidGenerator guidGenerator,
            IPermissionDataSeeder permissionDataSeeder,
            IApiScopeRepository apiScopeRepository)
        {
            _roleRepository = roleRepository;
            _clientRepository = clientRepository;
            _apiResourceRepository = apiResourceRepository;
            _identityResourceDataSeeder = identityResourceDataSeeder;
            _guidGenerator = guidGenerator;
            _permissionDataSeeder = permissionDataSeeder;
            _apiScopeRepository = apiScopeRepository;
        }

        [UnitOfWork]
        public virtual async Task SeedAsync(DataSeedContext context)
        {
            await _identityResourceDataSeeder.CreateStandardResourcesAsync();
            await CreateApiScopesAsync();
            await CreateApiResourcesAsync();
            await CreateClientsAsync();
            await SeedRolesAsync();
        }
        private async Task SeedRolesAsync()
        {

            if (await _roleRepository.GetCountAsync() > 1)
            {
                return;
            }
            await _roleRepository.InsertAsync(
               new IdentityRole
               (
                 _guidGenerator.Create(),
                  "Creator"
               ),
               autoSave: true
           );
            await _roleRepository.InsertAsync(
               new IdentityRole
               (
                 _guidGenerator.Create(),
                  "Supervisor"
               ),
               autoSave: true
           );
            await _roleRepository.InsertAsync(
           new IdentityRole
           (
             _guidGenerator.Create(),
              "Releaser"
           ),
           autoSave: true
       );
            await _roleRepository.InsertAsync(
                   new IdentityRole
                   (
                     _guidGenerator.Create(),
                      "AmlChecker"
                   ),
                   autoSave: true
               );
        }
        private async Task CreateApiScopesAsync()
        {
            await CreateApiScopeAsync("IdentityService");
            await CreateApiScopeAsync("TenantManagementService");
            await CreateApiScopeAsync("BloggingService");
            await CreateApiScopeAsync("ProductService");

            await CreateApiScopeAsync("RemittanceService");
            await CreateApiScopeAsync("CurrencyService");
            await CreateApiScopeAsync("CustomerService");
            await CreateApiScopeAsync("AmlService");
            
            await CreateApiScopeAsync("InternalGateway");
            await CreateApiScopeAsync("BackendAdminAppGateway");
            await CreateApiScopeAsync("PublicWebSiteGateway");
        }

        private async Task<ApiScope> CreateApiScopeAsync(string name)
        {
            var apiScope = await _apiScopeRepository.FindByNameAsync(name);
            if (apiScope == null)
            {
                apiScope = await _apiScopeRepository.InsertAsync(
                    new ApiScope(
                        _guidGenerator.Create(),
                        name,
                        name + " API"
                    ),
                    autoSave: true
                );
            }

            return apiScope;
        }

        private async Task CreateApiResourcesAsync()
        {
            var commonApiUserClaims = new[]
            {
                "email",
                "email_verified",
                "name",
                "phone_number",
                "phone_number_verified",
                "role"
            };

            await CreateApiResourceAsync("IdentityService", commonApiUserClaims);
            await CreateApiResourceAsync("TenantManagementService", commonApiUserClaims);
            await CreateApiResourceAsync("BloggingService", commonApiUserClaims);
            await CreateApiResourceAsync("ProductService", commonApiUserClaims);

            await CreateApiResourceAsync("RemittanceService", commonApiUserClaims);
            await CreateApiResourceAsync("CurrencyService", commonApiUserClaims);
            await CreateApiResourceAsync("CustomerService", commonApiUserClaims);
            await CreateApiResourceAsync("AmlService", commonApiUserClaims);

            await CreateApiResourceAsync("InternalGateway", commonApiUserClaims);
            await CreateApiResourceAsync("BackendAdminAppGateway", commonApiUserClaims);
            await CreateApiResourceAsync("PublicWebSiteGateway", commonApiUserClaims);
        }

        private async Task<ApiResource> CreateApiResourceAsync(string name, IEnumerable<string> claims)
        {
            var apiResource = await _apiResourceRepository.FindByNameAsync(name);
            if (apiResource == null)
            {
                apiResource = await _apiResourceRepository.InsertAsync(
                    new ApiResource(
                        _guidGenerator.Create(),
                        name,
                        name + " API"
                    ),
                    autoSave: true
                );
            }

            foreach (var claim in claims)
            {
                if (apiResource.FindClaim(claim) == null)
                {
                    apiResource.AddUserClaim(claim);
                }
            }

            return await _apiResourceRepository.UpdateAsync(apiResource);
        }

        private async Task CreateClientsAsync()
        {
            const string commonSecret = "E5Xd4yMqjP5kjWFKrYgySBju6JVfCzMyFp7n2QmMrME=";

            var commonScopes = new[]
            {
                "email",
                "openid",
                "profile",
                "role",
                "phone",
                "address"
            };

            await CreateClientAsync(
                "console-client-demo",
                new[] { "BloggingService", "IdentityService", "InternalGateway", "ProductService",  "TenantManagementService" },
                new[] { "client_credentials", "password" },
                commonSecret,
                permissions: new[] { IdentityPermissions.Users.Default, TenantManagementPermissions.Tenants.Default, "ProductManagement.Product" }
            );

            await CreateClientAsync(
                "backend-admin-app-client",
                commonScopes.Union(new[] { "BackendAdminAppGateway", "IdentityService", 
                    "ProductService", "RemittanceService","CurrencyService","CustomerService","AmlService", "TenantManagementService" }),
                new[] { "hybrid" },
                commonSecret,
                permissions: new[] { IdentityPermissions.Users.Default, "ProductManagement.Product", "RemittanceManagement.Remittance","CurrencyManagment.Currencies",
                "CustomerManagement.Customers","AmlManagement.AmlRemittance" },
                redirectUri: "https://localhost:44354/signin-oidc",
                postLogoutRedirectUri: "https://localhost:44354/signout-callback-oidc"
            );

            await CreateClientAsync(
                "public-website-client",
                commonScopes.Union(new[] { "PublicWebSiteGateway", "BloggingService", "ProductService" }),
                new[] { "hybrid" },
                commonSecret,
                redirectUri: "https://localhost:44335/signin-oidc",
                postLogoutRedirectUri: "https://localhost:44335/signout-callback-oidc"
            );

            await CreateClientAsync(
                "blogging-service-client",
                new[] { "InternalGateway", "IdentityService" },
                new[] { "client_credentials" },
                commonSecret,
                permissions: new[] { IdentityPermissions.UserLookup.Default }
            );
            await CreateClientAsync(
              "remittance-service-client",
              new[] { "InternalGateway" ,"IdentityService", "CustomerService", "CurrencyService", "AmlService" },
              new[] { "client_credentials" },
              commonSecret,
              permissions: new[] { "RemittanceManagement.Remittance", "CurrencyManagment.Currencies",
                  "CustomerManagement.Customers","AmlManagement.AmlRemittance" }
          );
            await CreateClientAsync(
            "currency-service-client",
            new[] { "InternalGateway", "IdentityService" },
            new[] { "client_credentials" },
            commonSecret,
            permissions: new[] { "CurrencyManagment.Currencies" }
        );
            await CreateClientAsync(
          "customer-service-client",
          new[] { "InternalGateway", "IdentityService" },
          new[] { "client_credentials" },
          commonSecret,
          permissions: new[] { "CustomerManagement.Customers" }
      );
            await CreateClientAsync(
            "aml-service-client",
            new[] { "InternalGateway", "IdentityService", "RemittanceService" },
            new[] { "client_credentials" },
            commonSecret,
             permissions: new[] { "AmlManagement.AmlRemittance", "RemittanceManagement.Remittance" }
        );
        }

        private async Task<Client> CreateClientAsync(
            string name,
            IEnumerable<string> scopes,
            IEnumerable<string> grantTypes,
            string secret,
            string redirectUri = null,
            string postLogoutRedirectUri = null,
            IEnumerable<string> permissions = null)
        {
            var client = await _clientRepository.FindByClientIdAsync(name);
            if (client == null)
            {
                client = await _clientRepository.InsertAsync(
                    new Client(
                        _guidGenerator.Create(),
                        name
                    )
                    {
                        ClientName = name,
                        ProtocolType = "oidc",
                        Description = name,
                        AlwaysIncludeUserClaimsInIdToken = true,
                        AllowOfflineAccess = true,
                        AbsoluteRefreshTokenLifetime = 31536000, //365 days
                        AccessTokenLifetime = 31536000, //365 days
                        AuthorizationCodeLifetime = 300,
                        IdentityTokenLifetime = 300,
                        RequireConsent = false,
                        RequirePkce = false
                    },
                    autoSave: true
                );
            }

            foreach (var scope in scopes)
            {
                if (client.FindScope(scope) == null)
                {
                    client.AddScope(scope);
                }
            }

            foreach (var grantType in grantTypes)
            {
                if (client.FindGrantType(grantType) == null)
                {
                    client.AddGrantType(grantType);
                }
            }

            if (client.FindSecret(secret) == null)
            {
                client.AddSecret(secret);
            }

            if (redirectUri != null)
            {
                if (client.FindRedirectUri(redirectUri) == null)
                {
                    client.AddRedirectUri(redirectUri);
                }
            }

            if (postLogoutRedirectUri != null)
            {
                if (client.FindPostLogoutRedirectUri(postLogoutRedirectUri) == null)
                {
                    client.AddPostLogoutRedirectUri(postLogoutRedirectUri);
                }
            }

            if (permissions != null)
            {
                await _permissionDataSeeder.SeedAsync(
                    ClientPermissionValueProvider.ProviderName,
                    name,
                    permissions
                );
            }

            return await _clientRepository.UpdateAsync(client);
        }
    }
}
