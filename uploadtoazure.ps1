param ([string]$SetupProfile="")

#param (
#    [string]$deleteExistingWebApps="false")

#if ([System.Convert]::ToBoolean($INI.azure.COverwrite)){

Clear-Host

$ErrorActionPreference = "Stop"

Import-Module -Force .\ComparionMod.psm1

$startTime = Get-Date
Copy-Item $SetupProfile .\setup.ini
$INI = Get-IniContent .\setup.ini

$sourceDirComparion = "$pwd/Application"
$sourceDirResponsive = "$pwd/HTML5Application"
$destinationPath = "/site/wwwroot"
$location = $INI.azure.location
$subscription = $INI.azure.SUBSCRIPTIONID

$ServicePrincipalAppId = $INI.azure.ServicePrincipalAppId           #AD -> App Registration
$ServicePrincipalKey = $INI.azure.ServicePrincipalKey         #AD -> App Registration Keys
$tenantid = $INI.azure.TenantID                               #AD -> Properties -> DirectoryID

# Check if Windows Azure Powershell is avaiable 
write-host 'Checking to see if AzureRM powershell module is installed'
if ((Get-Module -ListAvailable AzureRm) -eq $null) { 
    throw "Windows Azure Powershell not found! Please install from http://www.windowsazure.com/en-us/downloads/#cmd-line-tools" 
} 

# Azure Security Principle Login
$SecurePassword = $ServicePrincipalKey | ConvertTo-SecureString -AsPlainText -Force
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $ServicePrincipalAppId, $SecurePassword
Add-AzureRmAccount  -Credential $cred -Tenant $tenantid -ServicePrincipal -EnvironmentName $INI.azure.EnviornmentName  -subscriptionid $subscription
Select-AzureRmSubscription  -SubscriptionId $subscription -Tenant $INI.Azure.TenantID

   
#Transform config files
$sections="setup", "azure"
Set-Config("build")

#REPLACE VARIABLES
foreach($sec in $sections) {
    foreach($c in Get-ChildItem -Path .\Application, .\HTML5Application -Recurse -Filter *.config) {
        $configFile = Get-Content $c.FullName -Raw
        foreach($v in $INI[$sec].Keys) {
            $k = '%'+$v+'%'
            $match =   [regex]::IsMatch($configFile.ToLower(), $k.ToLower()) 
            if($match) {
                $configFile = $configFile -ireplace [regex]::Escape($k.ToLower()), $INI[$sec][$v]
            }
        }
        Set-Content $c.FullName -Value $configFile 
    }
}

#Sign TeamTimeAssistantClient with mage
Write-Host 'Sign Clickonce application'
Try 
{
    .\Sign.ps1
    Copy-Item .\TeamTimeClient\Release\publish\*.* .\Application\TTA
}
Catch
{
    Write-Host "Sign failed."
}


#prepare variables for Azure website
$WebSiteName = $ini.setup.DNS_FQDN.split(".")[0] + 'AzureWebsite';
$WebSiteResponsiveName = $ini.setup.DNS_EVALSITE.split(".")[0] + 'AzureWebsite';
$WebSiteCustomDomain = $ini.setup.DNS_FQDN;
$WebSiteResponsiveCustomDomain = $ini.setup.DNS_EVALSITE;
$WebSiteSubDomain = $ini.setup.DNS_FQDN.split(".")[0];
$WebSiteResponsiveSubDomain = $ini.setup.DNS_EVALSITE.split(".")[0];
$WebSiteDomain= $ini.setup.DNS_FQDN.Substring($ini.setup.DNS_FQDN.IndexOf('.')+1,$ini.setup.DNS_FQDN.length - $ini.setup.DNS_FQDN.IndexOf('.')-1)
$WebSiteResponsiveDomain= $ini.setup.DNS_EVALSITE.Substring($ini.setup.DNS_EVALSITE.IndexOf('.')+1,$ini.setup.DNS_EVALSITE.length - $ini.setup.DNS_EVALSITE.IndexOf('.')-1) 
$CServicePlanName = $ini.setup.DNS_FQDN.split(".")[0] + "_ServicePlan"
$RServicePlanName = $ini.setup.DNS_EVALSITE.split(".")[0] + "_ServicePlan"
$DbServerName =  $ini.azure.SqlServerName
$ResourceGroupName = $ini.azure.ResourceGroupName
$ResourceGroupLocation = $ini.azure.ResourceGroupLocation
$SqlServerDatabaseName = $ini.setup.DB_COMPARION
$StorageAccountName = $ini.azure.StorageAccountName
$DBUserName = $ini.setup.SqlServerUser
$DBUserPass = $ini.setup.SqlServerPassword

# Create new, or get existing resource group
############################################
$myResourceGroup = Get-AzureRmResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue

if(!$myResourceGroup) {
    Write-Output "Creating resource group: $ResourceGroupName"
    $myResourceGroup = New-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation
}
else {
    Write-Output "Resource group $ResourceGroupName already exists:"
}

if ($ini.azure.SqlServerName -ne "")
{
	# Create a new, or get existing server
	######################################
	$serverVersion = "12.0"

	$securePassword = ConvertTo-SecureString -String $DBUserPass -AsPlainText -Force
	$serverCreds = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $DBUserName, $securePassword


	$myServer = Get-AzureRmSqlServer -ServerName $DbServerName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
	if(!$myServer) {
		Write-Output "Creating SQL server: $DbServerName"
		$myServer = New-AzureRmSqlServer -ResourceGroupName $ResourceGroupName -ServerName $DbServerName -Location $ResourceGroupLocation -ServerVersion $serverVersion -SqlAdministratorCredentials $serverCreds
		Start-Sleep -Seconds 10
	}
	else {
		Write-Output "SQL server $DbServerName already exists:"
	}

    $FireWallRuleName = "AllowAllAzureIPs"
    $azureSqlDatabaseServerFirewallRule = Get-AzureRmSqlServerFirewallRule -ServerName $DbServerName -ResourceGroupName $ResourceGroupName -FirewallRuleName $FireWallRuleName  -ErrorAction SilentlyContinue
	if(!$azureSqlDatabaseServerFirewallRule) { 
		Write-Output "Creating AzureRmSqlServerFirewallRule to AllowAllAzureIPs"
        $azureSqlDatabaseServerFirewallRule = New-AzureRmSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $DbServerName  -AllowAllAzureIPs
	} 

	# Create a new, or get existing database
	######################################
	$MyDatabaseEdition = "Basic"
	$MyDatabaseServiceLevel = "Basic"

	#TODO: Add database backup plan
}

$AllHostingPlans = Get-AzureRmAppServicePlan
        
#Setup Comparion ServicePlan and WebApp
$HostingPlan = $AllHostingPlans | where {$_.Name -eq $CServicePlanName}
if($HostingPlan) {
    if ($HostingPlan.SKU -ne $INI.Azure.CTier -or $HostingPlan.WorkerSize -ne $INI.Azure.CServicePlanSize -or $HostingPlan.NumberOfWorkers -ne $INI.Azure.CNumberOfWorkers){
        $webapp = Get-AzureRMWebApp -ResourceGroupName $ResourceGroupName -Name $WebSiteName -ErrorAction SilentlyContinue
        if($webapp) {
            $webapp = Remove-AzureRmWebApp -Force -ResourceGroupName $ResourceGroupName  -Name $WebSiteName 
        }
        Write-Output "Removing existing ServicePlan $CServicePlanName"
        Remove-AzureRmAppServicePlan -Name $CServicePlanName -ResourceGroupName $ResourceGroupName -Force
        $HostingPlan = ""
    }
}
if(!$HostingPlan) {
    Write-Output "Creating ServicePlan $CServicePlanName"
    $CservicePlan = New-AzureRmAppServicePlan -ResourceGroupName $ResourceGroupName -Name $CServicePlanName -Location $ResourceGroupLocation -Tier $INI.azure.CTier -NumberofWorkers $INI.azure.CNumberOfWorkers -WorkerSize $INI.azure.CservicePlanSize
    Write-Output "Creating WebApp $WebSiteName"
    $webapp = New-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $WebSiteName -Location $ResourceGroupLocation -AppServicePlan $CServicePlanName
    Set-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $WebSiteName -HttpsOnly 1
}

#Setup Responsive ServicePlan and WebApp
$HostingPlan = $AllHostingPlans | where {$_.Name -eq $RServicePlanName}
if($HostingPlan) {
    if ($HostingPlan.SKU -ne $INI.Azure.RTier -or $HostingPlan.WorkerSize -ne $INI.Azure.RServicePlanSize -or $HostingPlan.NumberOfWorkers -ne $INI.Azure.RNumberOfWorkers){
        $webapp = Get-AzureRMWebApp -ResourceGroupName $ResourceGroupName -Name $WebSiteResponsiveName -ErrorAction SilentlyContinue
        if($webapp) {
            $webapp = Remove-AzureRmWebApp -Force -ResourceGroupName $ResourceGroupName  -Name $WebSiteResponsiveName
        }
        Write-Output "Removing existing ServicePlan $RServicePlanName"
        Remove-AzureRmAppServicePlan -Name $RServicePlanName -ResourceGroupName $ResourceGroupName -Force
        $HostingPlan = ""
    }
}
if(!$HostingPlan) {
    Write-Output "Creating ServicePlan $RServicePlanName"
    $RservicePlan = New-AzureRmAppServicePlan -ResourceGroupName $ResourceGroupName -Name $RServicePlanName -Location $ResourceGroupLocation -Tier $INI.azure.RTier -NumberofWorkers $INI.azure.RNumberOfWorkers -WorkerSize $INI.azure.RservicePlanSize
    Write-Output "Creating WebApp $WebSiteResponsiveName"
    $webapp = New-AzureRmWebApp -ResourceGroupName $ResourceGroupName  -Name $WebSiteResponsiveName -Location $ResourceGroupLocation -AppServicePlan $RServicePlanName
    Set-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $WebSiteResponsiveName -HttpsOnly 1
}

# Create virtual apps
######################################
$ConfigObject=@{
    virtualApplications = @(
        @{ virtualPath = "/"; physicalPath = "site\wwwroot" },
        @{ virtualPath = "cwsw"; physicalPath = "site\wwwroot\cwsw" },
        @{ virtualPath = "TeamTime"; physicalPath = "site\wwwroot\TeamTime" }
    )
}

Set-AzureRmResource -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Web/sites/Config -Name $WebSiteName/web -PropertyObject $ConfigObject -ApiVersion "2015-08-01" -Force

#Create DNS Simple Records
######################################
Clear-DnsClientCache 
$dnsresponse = Set-DNSSimpleCNameRecord -subdomain $WebSiteSubDomain -domain $WebSiteDomain -azureWebsiteDomain ("$($WebSiteName)" + "." + $ini.Azure.AzureBaseAddress) -account $ini.dnsimple.AccountId -token $ini.dnsimple.Token
Write-Output "CNAME created pointing :  $WebSiteSubDomain.$WebSiteDomain  -> $WebSiteName.$($ini.azure.AzureBaseAddress)"
$dnsresponse = Set-DNSSimpleCNameRecord -subdomain $WebSiteResponsiveSubDomain  -domain $WebSiteDomain -azureWebsiteDomain ("$($WebSiteResponsiveName)" + "." + $ini.Azure.AzureBaseAddress) -account $ini.dnsimple.AccountId -token $ini.dnsimple.Token
Write-Output "CNAME created pointing :  $WebSiteResponsiveSubDomain.$WebSiteResponsiveDomain  -> $WebSiteResponsiveName.$($ini.azure.AzureBaseAddress)"
Clear-DnsClientCache 

# Create a new, or get storage account
######################################
# Specify the type of Storage account to create and another type that will be used for updating the Storage account. Valid values are:
# Standard_LRS (locally-redundant storage)
# Standard_ZRS (zone-redundant storage)
# Standard_GRS (geo-redundant storage)
# Standard_RAGRS (read access geo-redundant storage)
# Premium_LRS (premium locally-redundant storage)

$StorageAccountType = 'Standard_LRS';
$storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName -ErrorAction SilentlyContinue
           
if(!$storageAccount) {
    $StorageAccountNameExists = Get-AzureRmStorageAccountNameAvailability -Name $StorageAccountName
     
    If (-Not $StorageAccountNameExists.NameAvailable) {  
        Write-Output "Storage account name '$StorageAccountName' already taken or invalid name";
        Exit 1
    }
    else {
        Write-Output "Creating Storage Account $StorageAccountName"
        $storageAccount = New-AzureRmStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName -Location $ResourceGroupLocation -Type $StorageAccountType
    }       
}
else {
    Write-Output "Storage account $StorageAccountName already exists:"
}

# Construct a storage account app settings hashtable.
$storageAccountKeys = Get-AzureRmStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $StorageAccountName;
$storageAccountPrimaryKey = $storageAccountKeys[0].Value;
$storageSettings = @{'STORAGE_ACCOUNT_NAME' = $StorageAccountName; 
    'STORAGE_ACCESS_KEY'   = $storageAccountPrimaryKey;
    'WEBSITE_LOAD_CERTIFICATES'   = "*";
}

#ComparionWebsite
$hostnames = @(("$($WebSiteName)" + "." + $ini.Azure.AzureBaseAddress), $WebSiteCustomDomain)
Set-AzureRMWebApp -Name $WebSiteName -ResourceGroupName $ResourceGroupName -AppSettings $storageSettings -HostNames $hostnames
write-host "Starting Upload of website $($WebSiteName)"
Publish-Directory -siteName $WebSiteName -sourcePath $sourceDirComparion -destinationPath $destinationPath -resourceGroupName $ResourceGroupName -azureBaseAddress $ini.Azure.AzureBaseAddress
$success = $?
if (!$success) {
    Write-Output "upload failed"
    Exit 1
}
#Upload SSL certificate
write-host "Uploading SSL Certificte:$($ini.azure.WCERT_CERTFILE)"
New-AzureRmWebAppSSLBinding -ResourceGroupName $ResourceGroupName -WebAppName $WebSiteName -CertificateFilePath $ini.azure.WCERT_CERTFILE -CertificatePassword $ini.azure.WCERT_CERTPASSWORD -Name "$WebSiteSubDomain.$WebSiteDomain"


#ResponsiveDesignWebsite
$hostnames = @(("$($WebSiteResponsiveName)" + "." + $ini.Azure.AzureBaseAddress), $WebSiteResponsiveCustomDomain)
Set-AzureRMWebApp -Name $WebSiteResponsiveName -ResourceGroupName $ResourceGroupName -AppSettings $storageSettings -HostNames $hostnames
write-host "Starting Upload of website $($WebSiteResponsiveName)"
Publish-Directory -siteName $WebSiteResponsiveName -sourcePath $sourceDirResponsive -destinationPath $destinationPath -resourceGroupName $ResourceGroupName -azureBaseAddress $ini.Azure.AzureBaseAddress
$success = $?
if (!$success) {
    Write-Output "upload failed"
    Exit 1
}
#Upload SSL certificate
write-host "Uploading SSL Certificte:$($ini.azure.WCERT_CERTFILE)"
New-AzureRmWebAppSSLBinding -SslState SniEnabled -ResourceGroupName $ResourceGroupName -WebAppName $WebSiteResponsiveName -CertificateFilePath $ini.azure.WCERT_CERTFILE -CertificatePassword $ini.azure.WCERT_CERTPASSWORD -Name "$WebSiteResponsiveSubDomain.$WebSiteResponsiveDomain"


#get-childitem -Path . -Filter *.config.default -Recurse | remove-item
#get-childitem -Path . -Filter *.config.build -Recurse | remove-item
#get-childitem -Path . -Filter *.config.transform -Recurse | remove-item
#get-childitem -Path . -Filter *.config.trusted -Recurse | remove-item

#### touch sites with http and https calls to force rebuild
Start-Job -ScriptBlock { param($url,$output) 
    Invoke-WebRequest -Uri ([System.Uri]($url)) -OutFile $output -ErrorAction Ignore
} -Argument  ('https://' + $WebSiteCustomDomain), (Join-Path $(Get-Location) 'tmp2.txt')
Start-Job -ScriptBlock { param($url,$output) 
    Invoke-WebRequest -Uri ([System.Uri]($url)) -OutFile $output -ErrorAction Ignore
} -Argument  ('https://' + $WebSiteCustomDomain + '/cwsw'), (Join-Path $(Get-Location) 'tmp2.txt')
Start-Job -ScriptBlock { param($url,$output) 
    Invoke-WebRequest -Uri ([System.Uri]($url)) -OutFile $output -ErrorAction Ignore
} -Argument  ('https://' + $WebSiteCustomDomain + '/cwsw'), (Join-Path $(Get-Location) 'tmp2.txt')
Start-Job -ScriptBlock { param($url,$output) 
    Invoke-WebRequest -Uri ([System.Uri]($url)) -OutFile $output -ErrorAction Ignore
} -Argument  ('https://' + $WebSiteResponsiveCustomDomain), (Join-Path $(Get-Location) 'tmp2.txt')
#### end touch sites block

# End Script
$finishTime = Get-Date
Write-Host (' Total time used (minutes): {0}' -f ($finishTime - $startTime).TotalMinutes)
# SIG # Begin signature block
# MIIaZgYJKoZIhvcNAQcCoIIaVzCCGlMCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUjqnC9Coa2kV0pFdxYGIfln9O
# Zj6gghQlMIIEfTCCA2WgAwIBAgIDG+cVMA0GCSqGSIb3DQEBCwUAMGMxCzAJBgNV
# BAYTAlVTMSEwHwYDVQQKExhUaGUgR28gRGFkZHkgR3JvdXAsIEluYy4xMTAvBgNV
# BAsTKEdvIERhZGR5IENsYXNzIDIgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwHhcN
# MTQwMTAxMDcwMDAwWhcNMzEwNTMwMDcwMDAwWjCBgzELMAkGA1UEBhMCVVMxEDAO
# BgNVBAgTB0FyaXpvbmExEzARBgNVBAcTClNjb3R0c2RhbGUxGjAYBgNVBAoTEUdv
# RGFkZHkuY29tLCBJbmMuMTEwLwYDVQQDEyhHbyBEYWRkeSBSb290IENlcnRpZmlj
# YXRlIEF1dGhvcml0eSAtIEcyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC
# AQEAv3FiCPH6WTT3G8kYo/eASVjpIoMTpsUgQwE7hPHmhUmfJ+r2hBtOoLTbcJjH
# MgGxBT4HTu70+k8vWTAi56sZVmvigAf88xZ1gDlRe+X5NbZ0TqmNghPktj+pA4P6
# or6KFWp/3gvDthkUBcrqw6gElDtGfDIN8wBmIsiNaW02jBEYt9OyHGC0OPoCjM7T
# 3UYH3go+6118yHz7sCtTpJJiaVElBWEaRIGMLKlDliPfrDqBmg4pxRyp6V0etp6e
# MAo5zvGIgPtLXcwy7IViQyU0AlYnAZG0O3AqP26x6JyIAX2f1PnbU21gnb8s51ir
# uF9G/M7EGwM8CetJMVxpRrPgRwIDAQABo4IBFzCCARMwDwYDVR0TAQH/BAUwAwEB
# /zAOBgNVHQ8BAf8EBAMCAQYwHQYDVR0OBBYEFDqahQcQZyi27/a9BUFuIMGU2g/e
# MB8GA1UdIwQYMBaAFNLEsNKR1EwRcbNhyz2h/t2oatTjMDQGCCsGAQUFBwEBBCgw
# JjAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AuZ29kYWRkeS5jb20vMDIGA1UdHwQr
# MCkwJ6AloCOGIWh0dHA6Ly9jcmwuZ29kYWRkeS5jb20vZ2Ryb290LmNybDBGBgNV
# HSAEPzA9MDsGBFUdIAAwMzAxBggrBgEFBQcCARYlaHR0cHM6Ly9jZXJ0cy5nb2Rh
# ZGR5LmNvbS9yZXBvc2l0b3J5LzANBgkqhkiG9w0BAQsFAAOCAQEAWQtTvZKGEack
# e+1bMc8dH2xwxbhuvk679r6XUOEwf7ooXGKUwuN+M/f7QnaF25UcjCJYdQkMiGVn
# OQoWCcWgOJekxSOTP7QYpgEGRJHjp2kntFolfzq3Ms3dhP8qOCkzpN1nsoX+oYgg
# HFCJyNwq9kIDN0zmiN/VryTyscPfzLXs4Jlet0lUIDyUGAzHHFIYSaRt4bNYC8nY
# 7NmuHDKOKHAN4v6mF56ED71XcLNa6R+ghlO773z/aQvgSMO3kwvIClTErF0UZzds
# yqUvMQg3qm5vjLyb4lddJIGvl5echK1srDdMZvNhkREg5L4wn3qkKQmw4TRfZHcY
# QFHfjDCmrzCCBJkwggOBoAMCAQICDxaI8DklXmOOaRQ5B+YzCzANBgkqhkiG9w0B
# AQUFADCBlTELMAkGA1UEBhMCVVMxCzAJBgNVBAgTAlVUMRcwFQYDVQQHEw5TYWx0
# IExha2UgQ2l0eTEeMBwGA1UEChMVVGhlIFVTRVJUUlVTVCBOZXR3b3JrMSEwHwYD
# VQQLExhodHRwOi8vd3d3LnVzZXJ0cnVzdC5jb20xHTAbBgNVBAMTFFVUTi1VU0VS
# Rmlyc3QtT2JqZWN0MB4XDTE1MTIzMTAwMDAwMFoXDTE5MDcwOTE4NDAzNlowgYQx
# CzAJBgNVBAYTAkdCMRswGQYDVQQIExJHcmVhdGVyIE1hbmNoZXN0ZXIxEDAOBgNV
# BAcTB1NhbGZvcmQxGjAYBgNVBAoTEUNPTU9ETyBDQSBMaW1pdGVkMSowKAYDVQQD
# EyFDT01PRE8gU0hBLTEgVGltZSBTdGFtcGluZyBTaWduZXIwggEiMA0GCSqGSIb3
# DQEBAQUAA4IBDwAwggEKAoIBAQDp6T3f1zcIyR44slJTQm0i8bHEBgRrnv2CdFBD
# fcagux9O+QJxJrHvQ9iDjEj85w+XeprrnN6mow47HEQYdY54pRdp/kkYpOK7XE7+
# jipUelDw1fbMkeeZedfeeZTXljP+DoO+Ir9jFiyj3Sgbrz2r6pfS8b8EEOc9SEX9
# H2hlwX9ZmWnAIjEMYm6nXGUBIbBjxCIYJ+7m/NIAPUcuqLiGVl0E3BMXJW4c30QP
# Fc2326VXdkJvAGiCmdLjwd7wi5RXTOwIkCIhziIrmAxC5kKTlJiT7/0G2T+8W5tU
# PCCx7mrWR3rFq4DpMJre8aQ/VU0KCTSKdSnSaa2XD1C/+MoJAgMBAAGjgfQwgfEw
# HwYDVR0jBBgwFoAU2u1kdBScFDyr3ZmpvVsoTYs8ydgwHQYDVR0OBBYEFI5rLTNr
# 9DOnk7MTmqXgCvcSNWqIMA4GA1UdDwEB/wQEAwIGwDAMBgNVHRMBAf8EAjAAMBYG
# A1UdJQEB/wQMMAoGCCsGAQUFBwMIMEIGA1UdHwQ7MDkwN6A1oDOGMWh0dHA6Ly9j
# cmwudXNlcnRydXN0LmNvbS9VVE4tVVNFUkZpcnN0LU9iamVjdC5jcmwwNQYIKwYB
# BQUHAQEEKTAnMCUGCCsGAQUFBzABhhlodHRwOi8vb2NzcC51c2VydHJ1c3QuY29t
# MA0GCSqGSIb3DQEBBQUAA4IBAQC6MyRAQIx821ifs2CYsvXAMf7rH25Q9grg5OaB
# rSaHot/9s9r0c/MA+ykbiRsVPttrUpMrxKw5gdc8Z1eaOTbgKAia4zlPm4kJf3vF
# YX9ZiTIlCmquGj7woieotsO4h/cWBEhBPVzY7J9NIDEE2WWh7c1pB1MWPd02AgqI
# 60DlBjALuBZL3O+8VQn/xj4SLnaz3M5C7/l2V+G3CgVAmFiaXXEWk3GMZYHqb/OJ
# 9/tzrbTnv9mOb6oLTyXzuOHV3XWYaIH4qsDRgMLExDmJwfbJnmzXdPnZl/hPwpoK
# zV6P+Bnp4KWfxPCSIeYteSXJIvnD8DqEV606FvRjlBAdXdDGMIIE0DCCA7igAwIB
# AgIBBzANBgkqhkiG9w0BAQsFADCBgzELMAkGA1UEBhMCVVMxEDAOBgNVBAgTB0Fy
# aXpvbmExEzARBgNVBAcTClNjb3R0c2RhbGUxGjAYBgNVBAoTEUdvRGFkZHkuY29t
# LCBJbmMuMTEwLwYDVQQDEyhHbyBEYWRkeSBSb290IENlcnRpZmljYXRlIEF1dGhv
# cml0eSAtIEcyMB4XDTExMDUwMzA3MDAwMFoXDTMxMDUwMzA3MDAwMFowgbQxCzAJ
# BgNVBAYTAlVTMRAwDgYDVQQIEwdBcml6b25hMRMwEQYDVQQHEwpTY290dHNkYWxl
# MRowGAYDVQQKExFHb0RhZGR5LmNvbSwgSW5jLjEtMCsGA1UECxMkaHR0cDovL2Nl
# cnRzLmdvZGFkZHkuY29tL3JlcG9zaXRvcnkvMTMwMQYDVQQDEypHbyBEYWRkeSBT
# ZWN1cmUgQ2VydGlmaWNhdGUgQXV0aG9yaXR5IC0gRzIwggEiMA0GCSqGSIb3DQEB
# AQUAA4IBDwAwggEKAoIBAQC54MsQ1K92vdSTYuswZLiBCGzDBNliF44v/z5lz4/O
# YuY8UhzaFkVLVat4a2ODYpDOD2lsmcgaFItMzEUz6ojcnqOvK/6AYZ15V8TPLvQ/
# MDxdR/yaFrzDN5ZBUY4RS1T4KL7QjL7wMDge87Am+GZHY23ecSZHjzhHU9FGHbTj
# 3ADqRay9vHHZqm8A29vNMDp5T19MR/gd71vCxJ1gO7GyQ5HYpDNO6rPWJ0+tJYql
# xvTV0KaudAVkV4i1RFXULSo6Pvi4vekyCgKUZMQWOlDxSq7neTOvDCAHf+jfBDnC
# aQJsY1L6d8EbyHSHyLmTGFBUNUtpTrw700kuH9zB0lL7AgMBAAGjggEaMIIBFjAP
# BgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB/wQEAwIBBjAdBgNVHQ4EFgQUQMK9J47M
# NIMwojPX+2yz8LQsgM4wHwYDVR0jBBgwFoAUOpqFBxBnKLbv9r0FQW4gwZTaD94w
# NAYIKwYBBQUHAQEEKDAmMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5nb2RhZGR5
# LmNvbS8wNQYDVR0fBC4wLDAqoCigJoYkaHR0cDovL2NybC5nb2RhZGR5LmNvbS9n
# ZHJvb3QtZzIuY3JsMEYGA1UdIAQ/MD0wOwYEVR0gADAzMDEGCCsGAQUFBwIBFiVo
# dHRwczovL2NlcnRzLmdvZGFkZHkuY29tL3JlcG9zaXRvcnkvMA0GCSqGSIb3DQEB
# CwUAA4IBAQAIfmyTEMg4uJapkEv/oV9PBO9sPpyIBslQj6Zz91cxG7685C/b+LrT
# W+C05+Z5Yg4MotdqY3MxtfWoSKQ7CC2iXZDXtHwlTxFWMMS2RJ17LJ3lXubvDGGq
# v+QqG+6EnriDfcFDzkSnE3ANkR/0yBOtg2DZ2HKocyQetawiDsoXiWJYRBuriSUB
# AA/NxBti21G00w9RKpv0vHP8ds42pM3Z2Czqrpv1KrKQ0U11GIo/ikGQI31bS/6k
# A1ibRrLDYGCD+H1QQc7CoZDDu+8CL9IVVO5EFdkKrqeKM+2xLXY2JtwE65/3YR8V
# 3Idv7kaWKK2hJn0KCacuBKONvPi8BDABMIIGLzCCBRegAwIBAgIJAM7cMgcIPv35
# MA0GCSqGSIb3DQEBCwUAMIG0MQswCQYDVQQGEwJVUzEQMA4GA1UECBMHQXJpem9u
# YTETMBEGA1UEBxMKU2NvdHRzZGFsZTEaMBgGA1UEChMRR29EYWRkeS5jb20sIElu
# Yy4xLTArBgNVBAsTJGh0dHA6Ly9jZXJ0cy5nb2RhZGR5LmNvbS9yZXBvc2l0b3J5
# LzEzMDEGA1UEAxMqR28gRGFkZHkgU2VjdXJlIENlcnRpZmljYXRlIEF1dGhvcml0
# eSAtIEcyMB4XDTE4MDcxNjEyMTAyNFoXDTIxMTAxMzExMzQwNFowcDELMAkGA1UE
# BhMCVVMxETAPBgNVBAgTCFZpcmdpbmlhMRIwEAYDVQQHEwlBcmxpbmd0b24xHDAa
# BgNVBAoTE0VYUEVSVCBDSE9JQ0UsIElOQy4xHDAaBgNVBAMTE0VYUEVSVCBDSE9J
# Q0UsIElOQy4wggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQC0H3Q9hQRT
# OBqnT41jmr/Q8IUollDjo5j3QYhOC0Mf4PLixht+GtXzz3ygmkAcgzYb2TurUFvZ
# z2yF0AqCcZnWmDmO5EeMO6yhxqu5aQNikuNAbfeRaCVcSBThvO9V2v3alDeGSoDF
# zfacICTWXF4GJ2uEHkh3zt5uVNwdoH4i6RMK+3Yp1w0734LnsLe+ZObw/3hteIGl
# eUap4vF8llXQptwL3lIMkATNiHj8b6EnL56tY2wYOl0BB9MhHut77oTGX/2yu+P/
# 3hcrkhBUCY5V3ofBdKMQse3CE70/HsdPj1OhHiqXzF2yKRO+SwQ3qA8XjTjLfmsD
# FiL6iWP94rIrMK+riBsR0hKateBN3Uz96ZeSG8zMSn6NH1DW7vYXIKwTRRe747Kr
# hS5S0exEAPt07Zm9TxP5IAP9OgT+PdbSnJQHUhg3brPJU7SC1i6XLeiBLGO1QLBD
# 0e2Qwo/99FDnq48e+p3b4/xkhyt3Y/62IC/Gco6aY7RtBvQdNhwIPO3NReIypI8h
# UrOL+3TKrnNJcXEqHR35K78gwPxydqHkvU5uhlSy2Nz4EzrbthNTpvsgMBOhSaJN
# OAzxk6Hxk+F7n0xilm5reQ6V7UiTSOyB6GenGiP2o7lIbTJcXz/ftMfKx4jQL9H3
# ZQjPlEj5aP+zT3PYPEL7TwHfCNEBcdMAtQIDAQABo4IBhTCCAYEwDAYDVR0TAQH/
# BAIwADATBgNVHSUEDDAKBggrBgEFBQcDAzAOBgNVHQ8BAf8EBAMCB4AwNQYDVR0f
# BC4wLDAqoCigJoYkaHR0cDovL2NybC5nb2RhZGR5LmNvbS9nZGlnMnM1LTQuY3Js
# MF0GA1UdIARWMFQwSAYLYIZIAYb9bQEHFwIwOTA3BggrBgEFBQcCARYraHR0cDov
# L2NlcnRpZmljYXRlcy5nb2RhZGR5LmNvbS9yZXBvc2l0b3J5LzAIBgZngQwBBAEw
# dgYIKwYBBQUHAQEEajBoMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5nb2RhZGR5
# LmNvbS8wQAYIKwYBBQUHMAKGNGh0dHA6Ly9jZXJ0aWZpY2F0ZXMuZ29kYWRkeS5j
# b20vcmVwb3NpdG9yeS9nZGlnMi5jcnQwHwYDVR0jBBgwFoAUQMK9J47MNIMwojPX
# +2yz8LQsgM4wHQYDVR0OBBYEFNxdWK6udUNYc/3gy5FIWDBZ8AIEMA0GCSqGSIb3
# DQEBCwUAA4IBAQAC1Ih6JOpB+d3HKh8yL/BN1nfto/jl3iUlfMl/xuwSzla0jjBL
# VB5OAnBjoDHW1CK1z6EaKsMVT+12o8ANYIDmp/RwkXS0MGdUmAHDm47ogo5lmGBu
# 6IlJBYMipOaH/AQgyg7bZLLaD/A9VfDr6ndZuZPipLK67xmBfGz/MdkElNEBYZlq
# V7o8w99vsc9xMX5BF8zNN/BTu2vdXzGlJs6EDJwocWL2ET9OxrRjDwvPTXd8PUu0
# iBEiEAvuR4YjUpfNhVV7JSmL0b9IxOc3gtywgzPjrmnULboAVLsvlox5k+IBXOLY
# MXqljzhpnf7/1eBzk9wQuRRwpBvTcbw/sNU4MYIFqzCCBacCAQEwgcIwgbQxCzAJ
# BgNVBAYTAlVTMRAwDgYDVQQIEwdBcml6b25hMRMwEQYDVQQHEwpTY290dHNkYWxl
# MRowGAYDVQQKExFHb0RhZGR5LmNvbSwgSW5jLjEtMCsGA1UECxMkaHR0cDovL2Nl
# cnRzLmdvZGFkZHkuY29tL3JlcG9zaXRvcnkvMTMwMQYDVQQDEypHbyBEYWRkeSBT
# ZWN1cmUgQ2VydGlmaWNhdGUgQXV0aG9yaXR5IC0gRzICCQDO3DIHCD79+TAJBgUr
# DgMCGgUAoHgwGAYKKwYBBAGCNwIBDDEKMAigAoAAoQKAADAZBgkqhkiG9w0BCQMx
# DAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAjBgkq
# hkiG9w0BCQQxFgQUgb0nC3+iCop3cWJGk3n8b7PHulowDQYJKoZIhvcNAQEBBQAE
# ggIAPgidRwGY3ZanI6DEa15fbq+qrwWtXYUSy9dfh5n7NFWvSKz4lLI8KZL+uBsn
# OqAJZKe/+o7q+nv9lmidIWWtN0/5XxVZ1ha+70RoHbVOzHt8+5YJ6ge+AN+n3v9f
# lQo+YJJFsFsl8YJqZi9Dz4r33Tl94AgiYP8RDsxnAPmJOAH/T4v6q+RkOEpWReJy
# TUBR98VdAS1RWJK1P3fOzP8OxO/F5Y3UBwTzXWnibFCPXx5prI+1M1MlYvuhhw+G
# Y0M2B6X6XcsisrQ6TSI25vknzdZqZ/K+Sgqt3bZ0LuU5KqxR+pfoQRez82uYdbYT
# Bcz5d9u+QUJSRitRIk11Nl0ppBzGXqC6Hnb7V5debFnTcRAN79cocPZp0GErh9yE
# JgHiBSS+Om4VtzuDgl1wyijCzwIqHRLcAWY6jOKT9sa3Ed7RXNzmfKuHQDwzaHUO
# rM6M9kBZG2HFZDEph1qvvOq6v/54wKneCgbFZRefQT0R2Ri630lSRdfRqn2qNso+
# KZTDx7IKI/obx8qgyPMgiFqpUvNpZkmWqH46rO7EchQngIwx1ebn0Gr7+Hqdds0R
# AXkGJeEIp6/0Lt+6GwAKXibzgaFVSD9RQuXoizAhaKrRNw5/HkcE1qUo6gPVd0OL
# q3VwCkDoRjX3r72FmDI7Z6KcK2jl6CR+2djGan/xrYnQqROhggJDMIICPwYJKoZI
# hvcNAQkGMYICMDCCAiwCAQEwgakwgZUxCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJV
# VDEXMBUGA1UEBxMOU2FsdCBMYWtlIENpdHkxHjAcBgNVBAoTFVRoZSBVU0VSVFJV
# U1QgTmV0d29yazEhMB8GA1UECxMYaHR0cDovL3d3dy51c2VydHJ1c3QuY29tMR0w
# GwYDVQQDExRVVE4tVVNFUkZpcnN0LU9iamVjdAIPFojwOSVeY45pFDkH5jMLMAkG
# BSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJ
# BTEPFw0xODEyMjIxODA3MTBaMCMGCSqGSIb3DQEJBDEWBBTnp8OmQgpJpcPdwJHD
# k/Dg6TmoRTANBgkqhkiG9w0BAQEFAASCAQAR5bMv4kysxCp3cCRXhsLWgeDKzJt2
# 5UQlxKfCwLmwIEGikjCw+fB1jmM6dvg1VyPck7sHJBvj7LtFpK6xWQMZMsfNHHr5
# FizFDA5PYiIa33RFMJYG/FOh16ku0gOIaj2ZoeNm28u9LBbcxdMbtqkNpvQW7Blf
# W0+qnVthd2GOR90c7cKLFRmYSxX5XQb1Rb6ED7OOJrdQzfwAX4Nq/8tMARiYGClp
# LKyGg4W5QHtiI4BXlzTrF3L59wgW+woVVVEWqVncVXM0emyOuEs8ttsUfYrVMa3E
# 2GHvbaZX32ezzuHk3v9ILfZcGKMzd5b8UTEr8+ta3eUdhA7nbfB1orpf
# SIG # End signature block
