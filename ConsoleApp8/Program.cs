using System;
using System.Runtime.InteropServices;
using System.Threading;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;
using Microsoft.Azure.Management.KeyVault.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Collections.Generic;

namespace ConsoleApp8
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var p = new Program();

            string clientId = "";
            string clientSecret = "";
            string tenantId = "";
            string subId = "";
            string location = "";
            string objectId = "";
            string rg = "";

            var vault = await p.GetVaultAsync(clientId, clientSecret, tenantId, subId, location, objectId, rg);

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var secretClient = new SecretClient(new Uri(vault.Properties.VaultUri), credential);
            var secret = new KeyVaultSecret("test", "value");
            await secretClient.SetSecretAsync(secret);
        }

        /// <summary>
        /// Create a vault to test.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="tenantId"></param>
        /// <param name="subId"></param>
        /// <param name="location"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        private async Task<VaultInner> GetVaultAsync(string clientId, string clientSecret, string tenantId, string subId, string location, string objectId, string rg)
        {
            var credential = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            var restClient = RestClient.Configure()
                .WithEnvironment(credential.Environment)
                .WithCredentials(credential)
                .Build();

            var client = new KeyVaultManagementClient(restClient);
            client.SubscriptionId = subId;

            var para = GetVaultCreateOrUpdateParameters(tenantId, location, objectId);

            var vault = await client.Vaults.CreateOrUpdateAsync(rg, "vault0512test", para);

            return vault;
        }

        /// <summary>
        /// Set the vault access policay can manage the secret
        /// </summary>
        /// <param name="location"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        private static VaultCreateOrUpdateParameters GetVaultCreateOrUpdateParameters(string tenantId, string location, string objectId)
        {
            var properties = new VaultProperties
            {
                TenantId = Guid.Parse(tenantId),
                Sku = new Sku(),
                AccessPolicies = new List<AccessPolicyEntry>(),
                CreateMode = CreateMode.Default
            };

            properties.AccessPolicies.Add(new AccessPolicyEntry
            {
                TenantId = properties.TenantId,
                ObjectId = objectId,
                Permissions = new Permissions
                {
                    Secrets = new SecretPermissions[] { SecretPermissions.Get, SecretPermissions.Set }
                }
            });

            return new VaultCreateOrUpdateParameters(location, properties);
        }
    }
}
