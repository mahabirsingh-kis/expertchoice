function New-Zip($zipfilename, $sourcedir)
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir,
        $zipfilename, $compressionLevel, $false)
}

function Set-Config([string]$transform = "default")
{
  $transform = $transform.ToLower().Trim()

  $files = Get-ChildItem  -Recurse -Filter *.config.$transform
  
  foreach($f in $files)
  {
    $dest = $f.FullName.Replace('.'+$transform,'')
    $transformFile = $f.FullName
    $source = $dest+'.default'
    & ($scriptdir + ".\ctt.exe") s:"$($source)" t:"$($transformFile)" d:"$($dest)" pw
  }

}

function Remove-Site([string]$configFile = $( throw "Missing required parameter configFile"))
{
    Set-Site -configFile $configFile -install 0
}

function New-Site([string]$configFile = $( throw "Missing required parameter configFile"))
{
    Set-Site -configFile $configFile -install 1
}

function Set-Site([Parameter(Mandatory)] [string]$configFile = $( throw "Missing required parameter configFile"), [Parameter(Mandatory)] [int] $install = 1)
{
    $INI = Get-IniContent $configFile
    $ComparionFQDN=$ini.setup.DNS_FQDN
    $ResponsiveFQDN=$ini.setup.DNS_EVALSITE
    $thumbprint = $ini.setup.SSL_THUMBPRINT.Trim()
    $AppPoolDotNetVersion = "v4.0"
    
	$ComparionAppPoolName = $ComparionFQDN
    $ComparionWebApp = $ComparionFQDN
    $ComparionAppName = $ComparionFQDN
    $ComparionWebAppDir = $PSScriptRoot + "\Application"

	$ResponsiveAppPoolName = $ResponsiveFQDN
    $ResponsiveWebApp = $ResponsiveFQDN
    $ResponsiveAppName = $ResponsiveFQDN
    $ResponsiveWebAppDir = $PSScriptRoot + "\HTML5Application"

    #remove website and application pool if they already exist
    Write-Host "removing " + $ComparionWebApp
    if (Test-Path IIS:\Sites\$ComparionWebApp)
    {
        Remove-Website -Name $ComparionWebApp 
    }
    if (Test-Path IIS:\AppPools\$ComparionAppPoolName)
    {
        Remove-WebAppPool -Name $ComparionAppPoolName
    }

	#remove responsive website and application pool if they already exist
    Write-Host "removing " + $ResponsiveWebApp
    if (Test-Path IIS:\Sites\$ResponsiveWebApp)
    {
        Remove-Website -Name $ResponsiveWebApp 
    }
    if (Test-Path IIS:\AppPools\$ResponsiveAppPoolName)
    {
        Remove-WebAppPool -Name $ResponsiveAppPoolName
    }

    if ($install -eq 0)
    {
        return
    }

    New-Item –Path IIS:\AppPools\$ComparionAppPoolName
    Set-ItemProperty -Path IIS:\AppPools\$ComparionAppPoolName -Name managedRuntimeVersion -Value 'v4.0'
    Set-ItemProperty -Path IIS:\AppPools\$ComparionAppPoolName -Name enable32BitAppOnWin64 -Value 'True'

    New-Website -Name $ComparionWebApp -PhysicalPath $ComparionWebAppDir -HostHeader $ComparionWebApp -ApplicationPool $ComparionWebApp

    New-WebBinding -Name $ComparionWebApp -IP "*" -Port 443 -Protocol https -HostHeader $ComparionWebApp
    $binding = Get-WebBinding -Name $ComparionWebApp -Protocol https
    $binding.AddSslCertificate($thumbprint, "My")

    ConvertTo-WebApplication IIS:\Sites\$ComparionWebApp\cwsw -ApplicationPool $ComparionAppPoolName
    ConvertTo-WebApplication IIS:\Sites\$ComparionWebApp\TeamTime -ApplicationPool $ComparionAppPoolName

    New-Item –Path IIS:\AppPools\$ResponsiveAppPoolName
    Set-ItemProperty -Path IIS:\AppPools\$ResponsiveAppPoolName -Name managedRuntimeVersion -Value 'v4.0'
    Set-ItemProperty -Path IIS:\AppPools\$ResponsiveAppPoolName -Name enable32BitAppOnWin64 -Value 'True'

    New-Website -Name $ResponsiveWebApp -PhysicalPath $ResponsiveWebAppDir -HostHeader $ResponsiveWebApp -ApplicationPool $ResponsiveWebApp

    New-WebBinding -Name $ResponsiveWebApp -IP "*" -Port 443 -Protocol https -HostHeader $ResponsiveWebApp
    $binding = Get-WebBinding -Name $ResponsiveWebApp -Protocol https 
    $binding.AddSslCertificate($thumbprint, "My")

}


function Grant-TeamTimeClient()
{

    #read from Setup.ini
    $INI = Get-IniContent .\Setup.ini

    $secSetup="setup"

    $FQDN= $INI[$secSetup]["DNS_FQDN"]
    $PUBLISHER= $INI[$secSetup]["PUBLISHER"]
    $CERTFILE= '.\' + $INI[$secSetup]["CERTFILE"]
    $CERTPASSWORD= $INI[$secSetup]["CERTPASSWORD"]
    $APPVERSION= $INI[$secSetup]["APPVERSION"]

    $FQDN -match ('[^.]*')
    $WebSiteName = $matches[0]
    
    if($WebSiteName -eq $null -or $FQDN -eq $null -or $APPVERSION -eq $null -or $PUBLISHER -eq $null -or $CERTFILE -eq $null -or $CERTPASSWORD -eq $null)
    {
      Write-Output 'missing parameters'
      Exit 1
    }

    $mage = '.\mage.exe'
    $APPNAME='TeamTimeAssistant-'+$WebSiteName
    $APPEXE='TeamTimeAssistant'
    $APPPATH='TeamTimeClient\Release\publish'
    $APPFILEPATH=$APPPATH + '\' + $APPEXE
    $MAINURL='http://' + $FQDN.ToLower().Trim()
    $PROVIDERURL=$MAINURL + '/TTA/TeamTimeAssistant.application'

    robocopy TeamTimeClient\Release $APPPATH

    get-childitem -Path $APPPATH -Filter *.pdb | remove-item
    get-childitem -Path $APPPATH -Filter *.exe.manifest | remove-item
    get-childitem -Path $APPPATH -Filter *.application | remove-item

    <#
        $TTAXML = '.\TeamTimeClient\TeamTimeAssistantService.xml'
        $TTAXMLValue = '<TTA serviceLocation="http://{0}/TeamTime/Keypad.svc/Client"/>' -f $MAINURL
        $TTAXMLValue | Out-File -Encoding Ascii -FilePath $TTAXML
    #>

    & $mage -New Application -ToFile $APPFILEPATH'.exe.manifest'  -Name $APPNAME    -Version $APPVERSION    -FromDirectory $APPPATH    -Processor x86
    & $mage -Sign $APPFILEPATH'.exe.manifest'  -CertFile $CERTFILE -Password $CERTPASSWORD
    & $mage -New Deployment -Publisher $PUBLISHER    -Install false -AppManifest $APPFILEPATH'.exe.manifest'  -Name $APPNAME    -ToFile $APPFILEPATH'.application'  -Version $APPVERSION    -providerURL $PROVIDERURL  -Processor x86

    foreach($file in (Get-Item $APPPATH'\*' -Include *.exe, *.dll, *.xml, *.config, *.ico, *.lib))
    {
      Rename-Item $file $($file.Name+'.deploy')
    }

    $appXMLFile = $APPFILEPATH+'.application'
    (Get-Content $appXMLFile) -ireplace [regex]::Escape('deployment install="false"'), 'deployment install="false" mapFileExtensions="true"' | Set-Content $appXMLFile

    & $mage -Sign $APPFILEPATH'.application' -CertFile $CERTFILE -Password $CERTPASSWORD

    robocopy $APPPATH Application\TTA /s

    Remove-Item -Path TeamTimeClient -Force -Recurse -ErrorAction Ignore

}


Function Get-IniContent {  
    <#  
      .Synopsis  
        Gets the content of an INI file  
          
      .Description  
        Gets the content of an INI file and returns it as a hashtable  
          
      .Notes  
        Author        : Oliver Lipkau <oliver@lipkau.net>  
        Blog        : http://oliver.lipkau.net/blog/  
        Source        : https://github.com/lipkau/PsIni 
                      http://gallery.technet.microsoft.com/scriptcenter/ea40c1ef-c856-434b-b8fb-ebd7a76e8d91 
        Version        : 1.0 - 2010/03/12 - Initial release  
                      1.1 - 2014/12/11 - Typo (Thx SLDR) 
                                         Typo (Thx Dave Stiff) 
          
        #Requires -Version 2.0  
          
      .Inputs  
        System.String  
          
      .Outputs  
        System.Collections.Hashtable  
          
      .Parameter FilePath  
        Specifies the path to the input file.  
          
      .Example  
        $FileContent = Get-IniContent "C:\myinifile.ini"  
        -----------  
        Description  
        Saves the content of the c:\myinifile.ini in a hashtable called $FileContent  
      
      .Example  
        $inifilepath | $FileContent = Get-IniContent  
        -----------  
        Description  
        Gets the content of the ini file passed through the pipe into a hashtable called $FileContent  
      
      .Example  
        C:\PS>$FileContent = Get-IniContent "c:\settings.ini"  
        C:\PS>$FileContent["Section"]["Key"]  
        -----------  
        Description  
        Returns the key "Key" of the section "Section" from the C:\settings.ini file  
          
      .Link  
        Out-IniFile  
    #>  
      
    [CmdletBinding()]  
    Param(  
        [ValidateNotNullOrEmpty()]  
        [ValidateScript({(Test-Path $_) -and ((Get-Item $_).Extension -eq ".ini")})]  
        [Parameter(ValueFromPipeline=$True,Mandatory=$True)]  
        [string]$FilePath  
    )  
      
    Begin  
        {Write-Verbose "$($MyInvocation.MyCommand.Name):: Function started"}  
          
    Process  
    {  
        Write-Verbose "$($MyInvocation.MyCommand.Name):: Processing file: $Filepath"  
              
        $ini = @{}  
        switch -regex -file $FilePath  
        {  
            "^\[(.+)\]$" # Section  
            {  
                $section = $matches[1]  
                $ini[$section] = @{}  
                $CommentCount = 0  
            }  
            "^(;.*)$" # Comment  
            {  
                if (!($section))  
                {  
                    $section = "No-Section"  
                    $ini[$section] = @{}  
                }  
                $value = $matches[1]  
                $CommentCount = $CommentCount + 1  
                $name = "Comment" + $CommentCount  
                $ini[$section][$name] = $value  
            }   
            "(.+?)\s*=\s*(.*)" # Key  
            {  
                if (!($section))  
                {  
                    $section = "No-Section"  
                    $ini[$section] = @{}  
                }  
                $name,$value = $matches[1..2]  
                $ini[$section][$name] = $value  
            }  
        }  
        Write-Verbose "$($MyInvocation.MyCommand.Name):: Finished Processing file: $FilePath"  
        Return $ini  
    }  
          
    End  
        {Write-Verbose "$($MyInvocation.MyCommand.Name):: Function ended"}  
} 

Function Publish-Directory
{
    param( 
	    [string]$siteName = $( throw "Missing required parameter siteName"),
	    [string]$sourcePath = $( throw "Missing required parameter sourcePath"),
	    [string]$destinationPath = $( throw "Missing required parameter destinationPath"),
        [string]$resourceGroupName = $( throw "Missing required parameter resourceGroupName"),
        [string]$azureBaseAddress = $( throw "Missing required parameter azureBaseAddress")
	)

    $zipFile = [System.IO.Path]::GetTempFileName() + ".zip"

    Add-Type -Assembly System.IO.Compression.FileSystem
    $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
    [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcePath, $zipFile, [System.IO.Compression.CompressionLevel]::Optimal, $false)

    Publish-Zip -siteName $siteName -sourceZipFile $zipFile -destinationPath $destinationPath -resourceGroupName $resourceGroupName -azureBaseAddress $azureBaseAddress
}
         

Function Publish-Zip
{
    param( 
        [string]$siteName = $( throw "Missing required parameter siteName"),
        [string]$sourceZipFile = $( throw "Missing required parameter sourceZipFile"),
        [string]$destinationPath = $( throw "Missing required parameter destinationPath"),
        [string]$resourceGroupName = $( throw "Missing required parameter resourceGroupName"),
        [string]$azureBaseAddress = $( throw "Missing required parameter azureBaseAddress")

    )

    [xml]$publishSettings = Get-AzureRmWebAppPublishingProfile -Format WebDeploy -OutputFile .\Temp.publishsettings -ResourceGroupName $resourceGroupName -Name $siteName
    $website = $publishSettings.SelectSingleNode("//publishData/publishProfile[@publishMethod='MSDeploy']")

    $timeOutSec = 900

    $username = $webSite.userName
    $password = $webSite.userPWD
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))

    $baseUrl = "https://" + $siteName + ".scm." + $azureBaseAddress
    $apiUrl = Join-Address ($baseUrl, "api/zip", $destinationPath) '/'

    Invoke-RestMethod -Uri $apiUrl -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method PUT -InFile $sourceZipFile -ContentType "multipart/form-data" -TimeoutSec $timeOutSec
}


Function Join-Address {
    param ([string[]] $Parts, [string] $Separator = '/')

    # example:
    #  Join-Address ('http://mysite','sub/subsub','/one/two/three') '/'

    $search = '(?<!:)' + [regex]::Escape($Separator) + '+'  #Replace multiples except in front of a colon for URLs.
    $replace = $Separator
    ($Parts | ? {$_ -and $_.Trim().Length}) -join $Separator -replace $search, $replace
}


Function GetCertKeys([Parameter(Mandatory)] [string] $certName)
{
    $cert = Get-ChildItem cert:\ -DNSNAME $certName -Recurse 
    if($cert -ne $null)
    {    
        $derFile = New-TemporaryFile
        $certFile = New-TemporaryFile
        Remove-Item $derFile, $certFile 
        Export-Certificate -Cert $('cert:\localMachine\my\' + $cert.Thumbprint) -FilePath $derFile.FullName -Type CERT -NoClobber | Out-Null
        certutil -encode $derFile.FullName $certFile | Out-Null

        $encodedvalue = Get-Content $certFile -Raw | Foreach {$_ -replace "`r`n",''}
        $header="-----BEGIN CERTIFICATE-----"
        $footer="-----END CERTIFICATE-----"
        $encodedvalue = $encodedvalue.Replace($header,"").Replace($footer,"").Trim()

        Remove-Item $derFile, $certFile -ErrorAction SilentlyContinue | Out-Null

        $f = $('.\' + $certName + '.txt')

        Remove-Item $f -ErrorAction SilentlyContinue
        Add-Content $f -value ("WCERT_encodedvalue=" + $encodedvalue.Trim())
        Add-Content $f -value ("WCERT_THUMBPRINT=" + $cert.Thumbprint.Trim())
        return $f
    }
}

Function Set-DNSSimpleCNameRecord(
  [Parameter(Mandatory=$true)]
  [String]$subdomain,
  [Parameter(Mandatory=$true)]
  [String]$domain,
  [Parameter(Mandatory=$true)]
  [String]$azureWebsiteDomain,
  [Parameter(Mandatory=$true)]
  [String]$account,
  [Parameter(Mandatory=$true)]
  [String]$token)
{

    if ($PSVersionTable.PSVersion.Major -ge 5)
    {
    }
    else
    {
       Write-Output "Requires PowerShell 5 or greater"
       exit 1;
    }

    #Get Zone Records

    $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records?name={2}",$account,$domain,$subdomain)
    $headers = @{}
    $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
    $headers["Accept"] = "application/json"
    $json = ""
    $json = Invoke-RestMethod $uri -Headers $headers
    $records = $json.data.Where({$_.type -eq 'CNAME'})
    $ttl = 300
    if ($records.Count -eq 0) 
    {
        $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records",$account,$domain)
        $headers = @{}
        $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
        $headers["Accept"] = "application/json"
        $json = @{}
        $json["name"]=$subdomain
        $json["type"]="CNAME"
        $json["ttl"]=$ttl
        $json["content"]=$azureWebsiteDomain
        $json = $json | ConvertTo-Json
        Invoke-WebRequest -Uri $uri -Body $json -Headers $headers -Method Post -ContentType "application/json"

    }
    else
    {
        $recordid = $records[0].id
        $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records/{2}",$account,$domain,$recordid)
        $headers = @{}
        $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
        $json = @{}
        $json["content"]=$azureWebsiteDomain
        $json["ttl"]=$ttl
        $json = $json | ConvertTo-Json
        Invoke-WebRequest -Uri $uri -Body $json -Headers $headers -Method Patch -ContentType "application/json"
    }


}

Function Set-DNSSimpleARecord(
  [Parameter(Mandatory=$true)]
  [String]$subdomain,
  [Parameter(Mandatory=$true)]
  [String]$domain,
  [Parameter(Mandatory=$true)]
  [String]$ip,
  [Parameter(Mandatory=$true)]
  [String]$account,
  [Parameter(Mandatory=$true)]
  [String]$token)
{

    if ($PSVersionTable.PSVersion.Major -ge 5)
    {
    }
    else
    {
       Write-Output "Requires PowerShell 5 or greater"
       exit 1;
    }

    #Get Zone Records

    $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records?name={2}",$account,$domain,$subdomain)
    $headers = @{}
    $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
    $headers["Accept"] = "application/json"
    $json = ""
    $json = Invoke-RestMethod $uri -Headers $headers
    $records = $json.data.Where({$_.type -eq 'A'})

    if ($records.Count -eq 0) 
    {
        $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records",$account,$domain)
        $headers = @{}
        $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
        $headers["Accept"] = "application/json"
        $json = @{}
        $json["name"]=$subdomain
        $json["type"]="A"
        $json["content"]=$ip
        $json = $json | ConvertTo-Json
        Invoke-WebRequest -Uri $uri -Body $json -Headers $headers -Method Post -ContentType "application/json"

    }
    else
    {
        $recordid = $records[0].id
        $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records/{2}",$account,$domain,$recordid)
        $headers = @{}
        $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
        $json = @{}
        $json["content"]=$ip
        $json = $json | ConvertTo-Json
        Invoke-WebRequest -Uri $uri -Body $json -Headers $headers -Method Patch -ContentType "application/json"
    }

}

Function Remove-DNSSimpleARecord(
  [Parameter(Mandatory=$true)]
  [String]$subdomain,
  [Parameter(Mandatory=$true)]
  [String]$domain,
  [Parameter(Mandatory=$true)]
  [String]$ip,
  [Parameter(Mandatory=$true)]
  [String]$account,
  [Parameter(Mandatory=$true)]
  [String]$token)
{

    if ($PSVersionTable.PSVersion.Major -ge 5)
    {
    }
    else
    {
       Write-Output "Requires PowerShell 5 or greater"
       exit 1;
    }

    #Get Zone Records

    $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records?name={2}",$account,$domain,$subdomain)
    $headers = @{}
    $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
    $headers["Accept"] = "application/json"
    $json = ""
    $json = Invoke-RestMethod $uri -Headers $headers
    $records = $json.data.Where({$_.type -eq 'A'})

    if ($records.Count -ne 0) 
    {
        $recordid = $records[0].id
        $uri =  [string]::Format("https://api.dnsimple.com/v2/{0}/zones/{1}/records/{2}",$account,$domain,$recordid)
        $headers = @{}
        $headers["Authorization"] = [string]::Format("Bearer {0}", $token)
        $json = @{}
        $json["content"]=$ip
        $json = $json | ConvertTo-Json
        Invoke-WebRequest -Uri $uri -Body $json -Headers $headers -Method Delete -ContentType "application/json"
    }

}

# SIG # Begin signature block
# MIIfoQYJKoZIhvcNAQcCoIIfkjCCH44CAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU2NzzALXCvow6T/Ha93Opc4qh
# Z9agghl7MIIEhDCCA2ygAwIBAgIQQhrylAmEGR9SCkvGJCanSzANBgkqhkiG9w0B
# AQUFADBvMQswCQYDVQQGEwJTRTEUMBIGA1UEChMLQWRkVHJ1c3QgQUIxJjAkBgNV
# BAsTHUFkZFRydXN0IEV4dGVybmFsIFRUUCBOZXR3b3JrMSIwIAYDVQQDExlBZGRU
# cnVzdCBFeHRlcm5hbCBDQSBSb290MB4XDTA1MDYwNzA4MDkxMFoXDTIwMDUzMDEw
# NDgzOFowgZUxCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJVVDEXMBUGA1UEBxMOU2Fs
# dCBMYWtlIENpdHkxHjAcBgNVBAoTFVRoZSBVU0VSVFJVU1QgTmV0d29yazEhMB8G
# A1UECxMYaHR0cDovL3d3dy51c2VydHJ1c3QuY29tMR0wGwYDVQQDExRVVE4tVVNF
# UkZpcnN0LU9iamVjdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAM6q
# gT+jo2F4qjEAVZURnicPHxzfOpuCaDDASmEd8S8O+r5596Uj71VRloTN2+O5bj4x
# 2AogZ8f02b+U60cEPgLOKqJdhwQJ9jCdGIqXsqoc/EHSoTbL+z2RuufZcDX65OeQ
# w5ujm9M89RKZd7G3CeBo5hy485RjiGpq/gt2yb70IuRnuasaXnfBhQfdDWy/7gbH
# d2pBnqcP1/vulBe3/IW+pKvEHDHd17bR5PDv3xaPslKT16HUiaEHLr/hARJCHhrh
# 2JU022R5KP+6LhHC5ehbkkj7RwvCbNqtMoNB86XlQXD9ZZBt+vpRxPm9lisZBCzT
# bafc8H9vg2XiaquHhnUCAwEAAaOB9DCB8TAfBgNVHSMEGDAWgBStvZh6NLQm9/rE
# JlTvA73gJMtUGjAdBgNVHQ4EFgQU2u1kdBScFDyr3ZmpvVsoTYs8ydgwDgYDVR0P
# AQH/BAQDAgEGMA8GA1UdEwEB/wQFMAMBAf8wEQYDVR0gBAowCDAGBgRVHSAAMEQG
# A1UdHwQ9MDswOaA3oDWGM2h0dHA6Ly9jcmwudXNlcnRydXN0LmNvbS9BZGRUcnVz
# dEV4dGVybmFsQ0FSb290LmNybDA1BggrBgEFBQcBAQQpMCcwJQYIKwYBBQUHMAGG
# GWh0dHA6Ly9vY3NwLnVzZXJ0cnVzdC5jb20wDQYJKoZIhvcNAQEFBQADggEBAE1C
# L6bBiusHgJBYRoz4GTlmKjxaLG3P1NmHVY15CxKIe0CP1cf4S41VFmOtt1fcOyu9
# 08FPHgOHS0Sb4+JARSbzJkkraoTxVHrUQtr802q7Zn7Knurpu9wHx8OSToM8gUmf
# ktUyCepJLqERcZo20sVOaLbLDhslFq9s3l122B9ysZMmhhfbGN6vRenf+5ivFBjt
# pF72iZRF8FUESt3/J90GSkD2tLzx5A+ZArv9XQ4uKMG+O18aP5cQhLwWPtijnGMd
# ZstcX9o+8w8KCTUi29vAPwD55g1dZ9H9oB4DK9lA977Mh2ZUgKajuPUZYtXSJrGY
# Ju6ay0SnRVqBlRUa9VEwggTQMIIDuKADAgECAgEHMA0GCSqGSIb3DQEBCwUAMIGD
# MQswCQYDVQQGEwJVUzEQMA4GA1UECBMHQXJpem9uYTETMBEGA1UEBxMKU2NvdHRz
# ZGFsZTEaMBgGA1UEChMRR29EYWRkeS5jb20sIEluYy4xMTAvBgNVBAMTKEdvIERh
# ZGR5IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9yaXR5IC0gRzIwHhcNMTEwNTAzMDcw
# MDAwWhcNMzEwNTAzMDcwMDAwWjCBtDELMAkGA1UEBhMCVVMxEDAOBgNVBAgTB0Fy
# aXpvbmExEzARBgNVBAcTClNjb3R0c2RhbGUxGjAYBgNVBAoTEUdvRGFkZHkuY29t
# LCBJbmMuMS0wKwYDVQQLEyRodHRwOi8vY2VydHMuZ29kYWRkeS5jb20vcmVwb3Np
# dG9yeS8xMzAxBgNVBAMTKkdvIERhZGR5IFNlY3VyZSBDZXJ0aWZpY2F0ZSBBdXRo
# b3JpdHkgLSBHMjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALngyxDU
# r3a91JNi6zBkuIEIbMME2WIXji//PmXPj85i5jxSHNoWRUtVq3hrY4NikM4PaWyZ
# yBoUi0zMRTPqiNyeo68r/oBhnXlXxM8u9D8wPF1H/JoWvMM3lkFRjhFLVPgovtCM
# vvAwOB7zsCb4Zkdjbd5xJkePOEdT0UYdtOPcAOpFrL28cdmqbwDb280wOnlPX0xH
# +B3vW8LEnWA7sbJDkdikM07qs9YnT60liqXG9NXQpq50BWRXiLVEVdQtKjo++Li9
# 6TIKApRkxBY6UPFKrud5M68MIAd/6N8EOcJpAmxjUvp3wRvIdIfIuZMYUFQ1S2lO
# vDvTSS4f3MHSUvsCAwEAAaOCARowggEWMA8GA1UdEwEB/wQFMAMBAf8wDgYDVR0P
# AQH/BAQDAgEGMB0GA1UdDgQWBBRAwr0njsw0gzCiM9f7bLPwtCyAzjAfBgNVHSME
# GDAWgBQ6moUHEGcotu/2vQVBbiDBlNoP3jA0BggrBgEFBQcBAQQoMCYwJAYIKwYB
# BQUHMAGGGGh0dHA6Ly9vY3NwLmdvZGFkZHkuY29tLzA1BgNVHR8ELjAsMCqgKKAm
# hiRodHRwOi8vY3JsLmdvZGFkZHkuY29tL2dkcm9vdC1nMi5jcmwwRgYDVR0gBD8w
# PTA7BgRVHSAAMDMwMQYIKwYBBQUHAgEWJWh0dHBzOi8vY2VydHMuZ29kYWRkeS5j
# b20vcmVwb3NpdG9yeS8wDQYJKoZIhvcNAQELBQADggEBAAh+bJMQyDi4lqmQS/+h
# X08E72w+nIgGyVCPpnP3VzEbvrzkL9v4utNb4LTn5nliDgyi12pjczG19ahIpDsI
# LaJdkNe0fCVPEVYwxLZEnXssneVe5u8MYaq/5Cob7oSeuIN9wUPORKcTcA2RH/TI
# E62DYNnYcqhzJB61rCIOyheJYlhEG6uJJQEAD83EG2LbUbTTD1Eqm/S8c/x2zjak
# zdnYLOqum/UqspDRTXUYij+KQZAjfVtL/qQDWJtGssNgYIP4fVBBzsKhkMO77wIv
# 0hVU7kQV2Qqup4oz7bEtdjYm3ATrn/dhHxXch2/uRpYoraEmfQoJpy4Eo428+LwE
# MAEwggTmMIIDzqADAgECAhBiXE2QjNVC+6supXM/8VQZMA0GCSqGSIb3DQEBBQUA
# MIGVMQswCQYDVQQGEwJVUzELMAkGA1UECBMCVVQxFzAVBgNVBAcTDlNhbHQgTGFr
# ZSBDaXR5MR4wHAYDVQQKExVUaGUgVVNFUlRSVVNUIE5ldHdvcmsxITAfBgNVBAsT
# GGh0dHA6Ly93d3cudXNlcnRydXN0LmNvbTEdMBsGA1UEAxMUVVROLVVTRVJGaXJz
# dC1PYmplY3QwHhcNMTEwNDI3MDAwMDAwWhcNMjAwNTMwMTA0ODM4WjB6MQswCQYD
# VQQGEwJHQjEbMBkGA1UECBMSR3JlYXRlciBNYW5jaGVzdGVyMRAwDgYDVQQHEwdT
# YWxmb3JkMRowGAYDVQQKExFDT01PRE8gQ0EgTGltaXRlZDEgMB4GA1UEAxMXQ09N
# T0RPIFRpbWUgU3RhbXBpbmcgQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
# AoIBAQCqgvGEqVvYcbXSXSvt9BMgDPmb6dGPdF5u7uspSNjIvizrCmFgzL2SjXzd
# dLsKnmhOqnUkcyeuN/MagqVtuMgJRkx+oYPp4gNgpCEQJ0CaWeFtrz6CryFpWW1j
# zM6x9haaeYOXOh0Mr8l90U7Yw0ahpZiqYM5V1BIR8zsLbMaIupUu76BGRTl8rOnj
# rehXl1/++8IJjf6OmqU/WUb8xy1dhIfwb1gmw/BC/FXeZb5nOGOzEbGhJe2pm75I
# 30x3wKoZC7b9So8seVWx/llaWm1VixxD9rFVcimJTUA/vn9JAV08m1wI+8ridRUF
# k50IYv+6Dduq+LW/EDLKcuoIJs0ZAgMBAAGjggFKMIIBRjAfBgNVHSMEGDAWgBTa
# 7WR0FJwUPKvdmam9WyhNizzJ2DAdBgNVHQ4EFgQUZCKGtkqJyQQP0ARYkiuzbj0e
# J2wwDgYDVR0PAQH/BAQDAgEGMBIGA1UdEwEB/wQIMAYBAf8CAQAwEwYDVR0lBAww
# CgYIKwYBBQUHAwgwEQYDVR0gBAowCDAGBgRVHSAAMEIGA1UdHwQ7MDkwN6A1oDOG
# MWh0dHA6Ly9jcmwudXNlcnRydXN0LmNvbS9VVE4tVVNFUkZpcnN0LU9iamVjdC5j
# cmwwdAYIKwYBBQUHAQEEaDBmMD0GCCsGAQUFBzAChjFodHRwOi8vY3J0LnVzZXJ0
# cnVzdC5jb20vVVROQWRkVHJ1c3RPYmplY3RfQ0EuY3J0MCUGCCsGAQUFBzABhhlo
# dHRwOi8vb2NzcC51c2VydHJ1c3QuY29tMA0GCSqGSIb3DQEBBQUAA4IBAQARyT3h
# Beg7ZazJdDEDt9qDOMaSuv3N+Ntjm30ekKSYyNlYaDS18AshU55ZRv1jhd/+R6pw
# 5D9eCJUoXxTx/SKucOS38bC2Vp+xZ7hog16oYNuYOfbcSV4Tp5BnS+Nu5+vwQ8fQ
# L33/llqnA9abVKAj06XCoI75T9GyBiH+IV0njKCv2bBS7vzI7bec8ckmONalMu1I
# l5RePeA9NbSwyVivx1j/YnQWkmRB2sqo64sDvcFOrh+RMrjhJDt77RRoCYaWKMk7
# yWwowiVp9UphreAn+FOndRWwUTGw8UH/PlomHmB+4uNqOZrE6u4/5rITP1UDBE0L
# kHLU6/u8h5BRsjgZMIIE/jCCA+agAwIBAgIQK3PbdGMRTFpbMkryMFdySTANBgkq
# hkiG9w0BAQUFADB6MQswCQYDVQQGEwJHQjEbMBkGA1UECBMSR3JlYXRlciBNYW5j
# aGVzdGVyMRAwDgYDVQQHEwdTYWxmb3JkMRowGAYDVQQKExFDT01PRE8gQ0EgTGlt
# aXRlZDEgMB4GA1UEAxMXQ09NT0RPIFRpbWUgU3RhbXBpbmcgQ0EwHhcNMTkwNTAy
# MDAwMDAwWhcNMjAwNTMwMTA0ODM4WjCBgzELMAkGA1UEBhMCR0IxGzAZBgNVBAgM
# EkdyZWF0ZXIgTWFuY2hlc3RlcjEQMA4GA1UEBwwHU2FsZm9yZDEYMBYGA1UECgwP
# U2VjdGlnbyBMaW1pdGVkMSswKQYDVQQDDCJTZWN0aWdvIFNIQS0xIFRpbWUgU3Rh
# bXBpbmcgU2lnbmVyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv1I2
# gjrcdDcNeNV/FlAZZu26GpnRYziaDGayQNungFC/aS42LwpnP0ChSopjNZvQGcx0
# qhcZkSu1VSAZ+8AaOm3KOZuC8rqVoRrYNMe4iXtwiHBRZmnsd/7GlHJ6zyWB7TSC
# mt8IFTcxtG2uHL8Y1Q3P/rXhxPuxR3Hp+u5jkezx7M5ZBBF8rgtgU+oq874vAg/Q
# TF0xEy8eaQ+Fm0WWwo0Si2euH69pqwaWgQDfkXyVHOaeGWTfdshgRC9J449/YGpF
# ORNEIaW6+5H6QUDtTQK0S3/f4uA9uKrzGthBg49/M+1BBuJ9nj9ThI0o2t12xr33
# jh44zcDLYCQD3npMqwIDAQABo4IBdDCCAXAwHwYDVR0jBBgwFoAUZCKGtkqJyQQP
# 0ARYkiuzbj0eJ2wwHQYDVR0OBBYEFK7u2WC6XvUsARL9jo2yVXI1Rm/xMA4GA1Ud
# DwEB/wQEAwIGwDAMBgNVHRMBAf8EAjAAMBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMI
# MEAGA1UdIAQ5MDcwNQYMKwYBBAGyMQECAQMIMCUwIwYIKwYBBQUHAgEWF2h0dHBz
# Oi8vc2VjdGlnby5jb20vQ1BTMEIGA1UdHwQ7MDkwN6A1oDOGMWh0dHA6Ly9jcmwu
# c2VjdGlnby5jb20vQ09NT0RPVGltZVN0YW1waW5nQ0FfMi5jcmwwcgYIKwYBBQUH
# AQEEZjBkMD0GCCsGAQUFBzAChjFodHRwOi8vY3J0LnNlY3RpZ28uY29tL0NPTU9E
# T1RpbWVTdGFtcGluZ0NBXzIuY3J0MCMGCCsGAQUFBzABhhdodHRwOi8vb2NzcC5z
# ZWN0aWdvLmNvbTANBgkqhkiG9w0BAQUFAAOCAQEAen+pStKwpBwdDZ0tXMauWt2P
# RR3wnlyQ9l6scP7T2c3kGaQKQ3VgaoOkw5mEIDG61v5MzxP4EPdUCX7q3NIuedcH
# TFS3tcmdsvDyHiQU0JzHyGeqC2K3tPEG5OfkIUsZMpk0uRlhdwozkGdswIhKkvWh
# QwHzrqJvyZW9ljj3g/etfCgf8zjfjiHIcWhTLcuuquIwF4MiKRi14YyJ6274fji7
# kE+5Xwc0EmuX1eY7kb4AFyFu4m38UnnvgSW6zxPQ+90rzYG2V4lO8N3zC0o0yoX/
# CLmWX+sRE+DhxQOtVxzhXZIGvhvIPD+lIJ9p0GnBxcLJPufFcvfqG5bilK+GLjCC
# Bi8wggUXoAMCAQICCQDO3DIHCD79+TANBgkqhkiG9w0BAQsFADCBtDELMAkGA1UE
# BhMCVVMxEDAOBgNVBAgTB0FyaXpvbmExEzARBgNVBAcTClNjb3R0c2RhbGUxGjAY
# BgNVBAoTEUdvRGFkZHkuY29tLCBJbmMuMS0wKwYDVQQLEyRodHRwOi8vY2VydHMu
# Z29kYWRkeS5jb20vcmVwb3NpdG9yeS8xMzAxBgNVBAMTKkdvIERhZGR5IFNlY3Vy
# ZSBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkgLSBHMjAeFw0xODA3MTYxMjEwMjRaFw0y
# MTEwMTMxMTM0MDRaMHAxCzAJBgNVBAYTAlVTMREwDwYDVQQIEwhWaXJnaW5pYTES
# MBAGA1UEBxMJQXJsaW5ndG9uMRwwGgYDVQQKExNFWFBFUlQgQ0hPSUNFLCBJTkMu
# MRwwGgYDVQQDExNFWFBFUlQgQ0hPSUNFLCBJTkMuMIICIjANBgkqhkiG9w0BAQEF
# AAOCAg8AMIICCgKCAgEAtB90PYUEUzgap0+NY5q/0PCFKJZQ46OY90GITgtDH+Dy
# 4sYbfhrV8898oJpAHIM2G9k7q1Bb2c9shdAKgnGZ1pg5juRHjDusocaruWkDYpLj
# QG33kWglXEgU4bzvVdr92pQ3hkqAxc32nCAk1lxeBidrhB5Id87eblTcHaB+IukT
# Cvt2KdcNO9+C57C3vmTm8P94bXiBpXlGqeLxfJZV0KbcC95SDJAEzYh4/G+hJy+e
# rWNsGDpdAQfTIR7re+6Exl/9srvj/94XK5IQVAmOVd6HwXSjELHtwhO9Px7HT49T
# oR4ql8xdsikTvksEN6gPF404y35rAxYi+olj/eKyKzCvq4gbEdISmrXgTd1M/emX
# khvMzEp+jR9Q1u72FyCsE0UXu+Oyq4UuUtHsRAD7dO2ZvU8T+SAD/ToE/j3W0pyU
# B1IYN26zyVO0gtYuly3ogSxjtUCwQ9HtkMKP/fRQ56uPHvqd2+P8ZIcrd2P+tiAv
# xnKOmmO0bQb0HTYcCDztzUXiMqSPIVKzi/t0yq5zSXFxKh0d+Su/IMD8cnah5L1O
# boZUstjc+BM627YTU6b7IDAToUmiTTgM8ZOh8ZPhe59MYpZua3kOle1Ik0jsgehn
# pxoj9qO5SG0yXF8/37THyseI0C/R92UIz5RI+Wj/s09z2DxC+08B3wjRAXHTALUC
# AwEAAaOCAYUwggGBMAwGA1UdEwEB/wQCMAAwEwYDVR0lBAwwCgYIKwYBBQUHAwMw
# DgYDVR0PAQH/BAQDAgeAMDUGA1UdHwQuMCwwKqAooCaGJGh0dHA6Ly9jcmwuZ29k
# YWRkeS5jb20vZ2RpZzJzNS00LmNybDBdBgNVHSAEVjBUMEgGC2CGSAGG/W0BBxcC
# MDkwNwYIKwYBBQUHAgEWK2h0dHA6Ly9jZXJ0aWZpY2F0ZXMuZ29kYWRkeS5jb20v
# cmVwb3NpdG9yeS8wCAYGZ4EMAQQBMHYGCCsGAQUFBwEBBGowaDAkBggrBgEFBQcw
# AYYYaHR0cDovL29jc3AuZ29kYWRkeS5jb20vMEAGCCsGAQUFBzAChjRodHRwOi8v
# Y2VydGlmaWNhdGVzLmdvZGFkZHkuY29tL3JlcG9zaXRvcnkvZ2RpZzIuY3J0MB8G
# A1UdIwQYMBaAFEDCvSeOzDSDMKIz1/tss/C0LIDOMB0GA1UdDgQWBBTcXViurnVD
# WHP94MuRSFgwWfACBDANBgkqhkiG9w0BAQsFAAOCAQEAAtSIeiTqQfndxyofMi/w
# TdZ37aP45d4lJXzJf8bsEs5WtI4wS1QeTgJwY6Ax1tQitc+hGirDFU/tdqPADWCA
# 5qf0cJF0tDBnVJgBw5uO6IKOZZhgbuiJSQWDIqTmh/wEIMoO22Sy2g/wPVXw6+p3
# WbmT4qSyuu8ZgXxs/zHZBJTRAWGZale6PMPfb7HPcTF+QRfMzTfwU7tr3V8xpSbO
# hAycKHFi9hE/Tsa0Yw8Lz013fD1LtIgRIhAL7keGI1KXzYVVeyUpi9G/SMTnN4Lc
# sIMz465p1C26AFS7L5aMeZPiAVzi2DF6pY84aZ3+/9Xgc5PcELkUcKQb03G8P7DV
# ODGCBZAwggWMAgEBMIHCMIG0MQswCQYDVQQGEwJVUzEQMA4GA1UECBMHQXJpem9u
# YTETMBEGA1UEBxMKU2NvdHRzZGFsZTEaMBgGA1UEChMRR29EYWRkeS5jb20sIElu
# Yy4xLTArBgNVBAsTJGh0dHA6Ly9jZXJ0cy5nb2RhZGR5LmNvbS9yZXBvc2l0b3J5
# LzEzMDEGA1UEAxMqR28gRGFkZHkgU2VjdXJlIENlcnRpZmljYXRlIEF1dGhvcml0
# eSAtIEcyAgkAztwyBwg+/fkwCQYFKw4DAhoFAKB4MBgGCisGAQQBgjcCAQwxCjAI
# oAKAAKECgAAwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIB
# CzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFAOhp2BwBDfeFl5V0GU0
# ourKZmDAMA0GCSqGSIb3DQEBAQUABIICAHQEthlcnVElCRCOfaeZUqG+uXiZ7EWp
# uY13pndNa9fWYXx+KVKxjUamT6+AcwJfJRFp9Ptwrqbx2o5a9JvIweBjmxTJmc5l
# cKUrmZKcRXl6MYPw/pE/meQPFEOuS3isrzlJ82FiEUEd07QEfkpS/q/OaR+tsmIm
# 6qTnLwBh/rwaDloFowgcq+h3bWY+ATdAmOUuv7s910gHSQz/qS4sxQti4jkJe/it
# eh5zJP9WCzNDl+2L9QVULIrINn3MyjrCDY4ISs3uyRl/Va3HW/Swn3saWDxpjvHL
# fmZi4dIFqmNSP91xVcLVqRrnofxIBzPo7F8GMqCsB2zUhcAr2x9xbwm1tMeJPEAu
# /XbEjP5Dpn12OoDe0fLGssaEF/r/af9cqLUXzdDi8FGYTVtOhu+efcbTIqCBKdwf
# xncsMHYySPyUrdSwCjHFG3ZwggJB6yLn2HHbw0Yj1pegRlpEkZ0hxAVEQd6lBoce
# KFsgHorlGxBeRoT8p4Km/jJOjPMseggWQoM9xHW1/bEHRhl/UY2HAug3jJTc3+UO
# PPvYYsXa2XhnlgE6NYQfrNSBFzlhXAR1J3GQWnf1cgp1mO/VPaAgNJ7F7xYafCDD
# K1aLuFdIvD36rsJojW/Xt7AoP4kQhxk10ACUYoo4+WeKLxSeLtuwt162a3nDrINa
# m2+dL8s3h4XqoYICKDCCAiQGCSqGSIb3DQEJBjGCAhUwggIRAgEBMIGOMHoxCzAJ
# BgNVBAYTAkdCMRswGQYDVQQIExJHcmVhdGVyIE1hbmNoZXN0ZXIxEDAOBgNVBAcT
# B1NhbGZvcmQxGjAYBgNVBAoTEUNPTU9ETyBDQSBMaW1pdGVkMSAwHgYDVQQDExdD
# T01PRE8gVGltZSBTdGFtcGluZyBDQQIQK3PbdGMRTFpbMkryMFdySTAJBgUrDgMC
# GgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcN
# MTkwODA2MTIxODA5WjAjBgkqhkiG9w0BCQQxFgQUEzZA9zOSbwD5Mf6Xdrs+abBj
# O0IwDQYJKoZIhvcNAQEBBQAEggEAQtQcK+39i17F5fhz04fSppzaJ7ZfFWZjErtg
# lANiuSJq+pKhc8WPosroSKmFijPMDDKW9NZvlh4I50bYlfP0SWacjzxkNKr6gQJQ
# Y+pkEaUfMwjqp+dlyy8AxJSkir3Hwuybtx9fhMlGIV85HDCGtj8btgAcVVruz/V0
# /5HioymwJ8m9VSyGa5b8NMC9ccxrHrZNtf1MDHsWWzwG7JDSgwjkI2p1/4oeHONe
# K44o5HctpvUZ0QMRy96t+0LyEjXa7WNsA1IqgNsxyQ4zhDL1Vo2hRCjv5h9YMA3B
# nKqUyUxbkyue0kCVO8PSZT4xMPnMQPcOEBp+0hXWQ2FhUjRKqw==
# SIG # End signature block
