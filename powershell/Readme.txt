Description:
This script copies the existing Azure CDN resources from one CDN provider to another CDN provider so that the seamless failover
between two CDN providers becomes possible. To switch between two CDN providers, You only need to change the CName mapping of the custom
domain from one Azure CDN Endpoint to another Azure CDN Endpoint. See "Failover step" for details.

Setup:
This script builds on top of Azure Powershell. You need to install Azure PowerShell(https://github.com/Azure/azure-powershell/releases)
and Run "Login-AzureRmAccount" first.

Prerequisite:
0. Setup Azure Powershell, login in the powershell and select the correct default subscription(Select-AzureRmSubscription)
1. A working Auzre CDN Endpoint.
2. (Optional)A list of custom domains that need to be setup on the endpoint. All the custom domains need to be registered with cdnverify(
   https://azure.microsoft.com/en-us/documentation/articles/cdn-map-content-to-custom-domain/#register-a-custom-domain-for-an-azure-cdn-endpoint-using-the-intermediary-cdnverify-subdomain)
   Note that the newly created endpoint will be named as <existingendpointname>_standardakamai.azureedge.net or 
   <existingendpointname>_standardverizon.azureedge.net based on the existing CDN provider. If the existing CDN Endpoint is with provider
   Akamai, then the newly created endpoint will be with provider Verizon

Input:
The name of the existing Azure CDN Endpoint that is currently working.
(Optional)A list of custom domain host names that need to be setup on the newly created endpoint.

Output:
Another Azure CDN Profile in a different CDN provider and an endpoint with same settings with the existing Azure CDN Endpoint.

Failover step:
After running this script, you can control which CDN you want to use by changing the CNAME mapping of the custom domain with your DNS Provider for the CNAME.
For example:
1. I already have a custom domain cdn.contoso.com points to contoso_standardverizon.azureedge.net
2. I copied the CDN resources with this script and I have a newly created endpoint named: contoso_standardakamai.azureedge.net I also
verified that it is working.
3. I change CName mapping of cdn.contoso.com to contoso_standardakamai.azureedge.net