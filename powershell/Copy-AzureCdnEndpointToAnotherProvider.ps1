# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

Param(
  # The name of the endpoint that will be replicated
  [Parameter(Mandatory=$True)] [ValidateNotNull()][string]$endpointName,
  [string[]]$customDomainHostNames
)

$profiles = Get-AzureRMCdnProfile
$profileIdRegex = "\/subscriptions\/(?<subscriptionId>.*)\/resourcegroups\/(?<resourceGroup>.*)\/providers\/Microsoft\.Cdn\/profiles\/(?<profileName>.*)"

$foundEndpoint = $False
$existingProfile = $null
$existingEndpoint = $null

# Looking for the parent profile and resource group of the specified endpoint
foreach($profile in $profiles) {
    $match = $profile.Id -match $profileIdRegex
    $resourceGroupName = $Matches['resourceGroup']
    
    try {
        # Check if the endpoint exists.
        $existingEndpoint = Get-AzureRmCdnEndpoint -EndpointName $endpointName `
                                                   -ProfileName $profile.Name `
                                                   -ResourceGroupName $resourceGroupName `
                                                   -ErrorAction Stop
        $existingProfile = $profile
        $foundEndpoint = $True
        break
    }
    catch {
        # swallow not found exception but re-throw any other exceptions
        if(!$_.Exception.Response.StatusCode -eq "NotFound") {
            throw
        }
    }
}

if(!$foundEndpoint) {
    Write-Error "Cannot find endpoint with name $endpointName"
    return
}

Write-Host "Found following Azure CDN Profile"
Write-Output $existingProfile

Write-Host "Found following Azure CDN Endpoint"
Write-Output $existingEndpoint

$skuValue = $existingProfile.Sku.Name
# Get the new sku based on the existing sku. If existing provider is Akamai, then the new provider will be Verizon and vice versa.
switch($skuValue)
{
    "StandardVerizon" {
        $newSku = "StandardAkamai"
    }

    "StandardAkamai" {
        $newSku = "StandardVerizon"
    }

    default {
        Write-Error "Cannot copy profile with sku '$existingProfile.Sku.Name' to another provider"
        return
    }
}

Write-Host "Creating new Azure CDN Profile and Azure CDN Endpoint"
$newProfileName = ($existingProfile.Name + "-" + $newSku).ToLower()
$newEndpointName = ($existingEndpoint.Name + "-" + $newSku).ToLower()
$newProfile = New-AzureRmCdnProfile -ProfileName $newProfileName -Location $existingProfile.Location -Sku $newSku -ResourceGroupName $resourceGroupName
$newEndpoint = New-AzureRmCdnEndpoint -EndpointName $newEndpointName `
                       -ProfileName $newProfileName `
                       -ResourceGroupName $resourceGroupName `
                       -Location $existingEndpoint.Location `
                       -OriginHostHeader $existingEndpoint.OriginHostHeader `
                       -OriginPath $existingEndpoint.OriginPath `
                       -ContentTypesToCompress $existingEndpoint.ContentTypesToCompress `
                       -IsCompressionEnabled $existingEndpoint.IsCompressionEnabled `
                       -IsHttpAllowed $existingEndpoint.IsHttpAllowed `
                       -IsHttpsAllowed $existingEndpoint.IsHttpsAllowed `
                       -QueryStringCachingBehavior $existingEndpoint.QueryStringCachingBehavior `
                       -OriginName $existingEndpoint.Origins[0].Name `
                       -OriginHostName $existingEndpoint.Origins[0].HostName `
                       -HttpPort $existingEndpoint.Origins[0].HttpPort `
                       -HttpsPort $existingEndpoint.Origins[0].HttpsPort

if($customDomainHostNames)
{
    $customDomainNamePrefix = "customdomain-"
    $counter = 0
    foreach ($customDomainHostName in $customDomainHostNames) {
        $customDomainName = $customDomainNamePrefix + $counter
        New-AzureRmCdnCustomDomain -HostName $customDomainHostName `
                                   -CustomDomainName $customDomainName `
                                   -EndpointName $newEndpointName `
                                   -ProfileName $newEndpoint `
                                   -ResourceGroupName $resourceGroupName
        $counter++
    }
}

Write-Host "Created following Azure CDN Profile and Azure CDN Endpoint"
Write-Output $newProfile
Write-Output $newEndpoint