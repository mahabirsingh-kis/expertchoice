param ([string]$loadIni="AzureIIS.ini", [string]$WebsiteName="mywebsite", [string]$DBname="mydb", [string]$SetupProfile="AzureIIS.ini")

Clear-Host
Push-Location (Split-Path $MyInvocation.MyCommand.Path)

Import-Module -Force .\ComparionMod.psm1
Remove-Item .\compile.txt -ErrorAction SilentlyContinue

.\WriteBuildInfo.ps1

$Xfer = Get-INIContent ([io.path]::combine(${env:userprofile}, 'XferPath.ini'))
$XferPath = $Xfer.Paths.XferPath

$BuildInfo = .\GetBuildVersion.ps1 
$Changeset = $BuildInfo.Changeset
$AppVersion = $BuildInfo.AppVersion


if(1) {
    $azInIni = ${env:userprofile}+"\" + $SetupProfile
    $AZOUTINI=""
    $AZINI = Get-INIContent $azInIni
    $AZINI.info.INFO_CHANGESET=$Changeset
    $AZINI.info.INFO_APPVERSION=$AppVersion
    foreach($line in Get-Content $azInIni) {
        if($line -match "\w*\[.*\]\w*"){
            $section = $Matches.Values
            $AZOUTINI=$AZOUTINI+$section+"`r`n"
            $ss = $section.replace('[','').replace(']','')
        }
        elseif($line -match "\S*(?=\s*=)") {
            $k = $Matches.Values
            $val = $AZINI[$ss][$k]
            $AZOUTINI=$AZOUTINI+$k+"=" + $val + "`r`n"
        }
    }

    Set-Content ([io.path]::combine(".\",$SetupProfile)) -Value $AZOUTINI
}


$aziniFile = 'azure.publish.ini'
$setupFile = $aziniFile
$envLoadIni = ${env:userprofile}+"\$loadIni"
if ($envLoadIni -ne ""){
    if (!(Test-Path $envLoadIni)){
        Write-Output $envLoadIni " not found"
        Exit 1
    }
    Write-Output "Copying " $envLoadIni
    Copy-Item $envLoadIni $setupFile
    $success = $?
} else { 
    if (!$success) {
        Write-Output ($setupFile + " not found")
        Exit 1
    }
} 

if ((Test-Path $XferPath)){
}
else {
    Write-Output -InputObject "Could not find Xfer path"
    Exit 1
}

$TargetPath = [io.path]::combine($XferPath, $Changeset)
$FullTP = [io.path]::combine($TargetPath,"Application_package\")
if(Test-Path $TargetPath){
#    if (Test-Path $FullTP){
#        Copy-Item ([io.path]::combine(".\",$SetupProfile)) ([io.path]::combine($XferPath, $Changeset, "Application_package\"))
#        Write-Output -InputObject "Success (rebuild not necessary)"
#        Exit 0
#    }
#    else {
        Remove-Item -Force -Recurse $TargetPath
        $success = $true
#    }
}

$success = $?
if($success){

    $OUTINI=""
    $INI = Get-INIContent $setupFile
    $INI.info.INFO_CHANGESET=$Changeset
    $INI.info.INFO_APPVERSION=$AppVersion

    foreach($line in Get-Content .\SelfHostSetup.template.ini) {
        if($line -match "\w*\[.*\]\w*"){
            $section = $Matches.Values
            $OUTINI=$OUTINI+$section+"`r`n"
            $ss = $section.replace('[','').replace(']','')
        }
        elseif($line -match "\S*(?=\s*=)") {
            $k = $Matches.Values
            $val = $INI[$ss][$k]
            $OUTINI=$OUTINI+$k+"=" + $val + "`r`n"
        }
    }

    if(1){
        $AZOUTINI=""
        $AZINI = Get-INIContent $aziniFile
        $AZINI.info.INFO_CHANGESET=$Changeset
        $AZINI.info.INFO_APPVERSION=$AppVersion

        foreach($line in Get-Content $aziniFile) {
            if($line -match "\w*\[.*\]\w*"){
                $section = $Matches.Values
                $AZOUTINI=$AZOUTINI+$section+"`r`n"
                $ss = $section.replace('[','').replace(']','')
            }
            elseif($line -match "\S*(?=\s*=)") {
                $k = $Matches.Values
                $val = $INI[$ss][$k]
                $AZOUTINI=$OUTINI+$k+"=" + $val + "`r`n"
            }
        }

        Set-Content ("new." + $aziniFile) -Value $AZOUTINI

        #$AZOUTINI.info.INFO_CHANGESET=$Changeset
        #$AZOUTINI.info.INFO_APPVERSION=$AppVersion
        #$AZINI = Get-INIContent $azini
        #Set-Content $azini -Value $AZOUTINI
    }
#    (Get-Content $setupFile) | Foreach-Object {$_ -replace 'azure-site-name',$WebsiteName} | Out-File $setupFile
#    (Get-Content $setupFile) | Foreach-Object {$_ -replace 'azure-db-name',$DBname} | Out-File $setupFile

    #(Get-Content $setupFile) | Foreach-Object {$_ -replace 'azure-site-name',$WebsiteName} | Foreach-Object {$_ -replace 'azure-db-name',$DBname} | Out-File $setupFile

    #$INI = Get-INIContent $setupFile
    #Copy-Item $setupFile ./setup.ini

#================
    $shini = 'SelfHostSetup.template.ini'
    $setupFile = $shini
    $OUTINI=""
    $INI = Get-INIContent $setupFile
    $INI.info.INFO_CHANGESET=$Changeset
    $INI.info.INFO_APPVERSION=$AppVersion

    foreach($line in Get-Content .\SelfHostSetup.template.ini) {
        if($line -match "\w*\[.*\]\w*"){
            $section = $Matches.Values
            $OUTINI=$OUTINI+$section+"`r`n"
            $ss = $section.replace('[','').replace(']','')
        }
        elseif($line -match "\S*(?=\s*=)") {
            $k = $Matches.Values
            $val = $INI[$ss][$k]
            $OUTINI=$OUTINI+$k+"=" + $val + "`r`n"
        }
    }

    $setupFile = "new." + $setupfile
    Set-Content $setupFile -Value $OUTINI
    $INI = Get-INIContent $setupFile

    #Set-Content $loadIni -Value $OUTINI

    #write out repo info
    $hgdate = hg log -r . --template "{date|date}"
    $hgidshort = $(hg id -i).Replace("+","").Trim()
    $hgbranchname = hg id -b -r $hgidshort
    $hgrev = $(hg identify --num).Replace("+","").Trim()
    $hgid = $hgidshort + "`t(" + $hgbranchname + ")"

    $hgid | Out-File -Encoding Ascii -FilePath "hgid.txt"
    $hgdate | Out-File -Encoding Ascii -FilePath "hgdate.txt"
    $hgrev | Out-File -Encoding Ascii -FilePath "revision.txt"

    $MSBUILD=  ${env:ProgramFiles(x86)}+"\MSBuild\14.0\Bin\MSBuild.exe" 
    $devenv=  ${env:ProgramFiles(x86)}+"\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com" 

    Remove-Item repo.txt -ErrorAction SilentlyContinue
    Add-Content repo.txt ("Build: " + $AppVersion)
    Add-Content repo.txt ("Hg: " + $hgid)
    Add-Content repo.txt ("Date: " + $hgdate)
    Add-Content repo.txt ("BuildDate: " + (Get-Date -Format G))
}

$success = $success -and $?

if ($success) {
    Set-Config("build")

    .\nuget.exe restore .\CanvasLocal.sln

    & $devenv .\CanvasLocal.sln /Deploy "Release|Mixed Platforms" | Out-File -Encoding Ascii -Append -FilePath compile.txt
    $success = $success -and $?
	
    $buildProjectName = ""

    if ($success) {
        $buildProjectName = "Application"
        & $MSBUILD ./Application\Application.vbproj /p:PublishProfile=Release /p:DeployOnBuild=true | Out-File -Encoding Ascii -Append -FilePath compile.txt
        $success = $success -and $?
#        if ($success) {
#            $buildProjectName = "TTAHttp"
#            & $MSBUILD ./TeamTimeAssistant\TTAHttp\TTAHttp.vbproj /p:PublishProfile=Release /p:DeployOnBuild=true | Out-File -Encoding Ascii -Append -FilePath compile.txt
#            $success = $success -and $?
#        }
    }

    if ($success) {
        New-Item -ItemType directory -Path Application_package
        Move-Item -Path Application_Published\Release -Destination Application_package\Application
        Remove-Item -Path Application_Published -Recurse

        #$AppVersion + '.' + $Changeset | Out-File -FilePath Application_package\folder.txt -Encoding ASCII
    }

    if (-not $success) {
        Write-Output -InputObject "COMPILE ERROR OCCURED AT $buildProjectName"
        Start-Process -FilePath notepad.exe -ArgumentList compile.txt
    }
    else {

        Remove-Item -Path .\Application_package\Application\App_Data\licenses -Force -Recurse
        Remove-Item -Path .\Application_package\Application\Special -Force -Recurse
        Remove-Item -Path .\Application_package\Application\Test -Force -Recurse
        Copy-Item .\repo.txt Application_package\Application\

        #copy TeamTimeAssistant
#        robocopy TeamTimeAssistant\TTAHttp\TTAHttp.vbproj_deploy\Release Application_package\Application\TeamTime /s
#        Copy-Item .\repo.txt Application_package\Application\TeamTime\
#        Copy-Item .\Application_package\Application\repo.htm Application_package\Application\TeamTime\
#        Remove-Item Application_package\Application\TeamTime\connections.config
#        Remove-Item Application_package\Application\TeamTime\appSettings.config
  
#        #copy TeamTimeClient
#        robocopy TeamTimeAssistant\TeamTimeAssistantClient\bin\Release Application_package\TeamTimeClient\Release /s
  
        #cleanup
        get-childitem -Path .\Application_package\ -Filter Web_VD.config -Recurse | remove-item
        #get-childitem -Path .\Application_package\ -Filter *.config.default -Recurse | remove-item
        #get-childitem -Path .\Application_package\ -Filter *.config.build -Recurse | remove-item

        #Add setup scripts to package
                 
        Copy-Item  ("new." +$aziniFile)  ('.\Application_package\' + $aziniFile)
        Copy-Item ("new." + $shini)  ('.\Application_package\' + $shini)
        Copy-Item $SetupProfile ('.\Application_package\' + $SetupProfile)

        Copy-Item .\AzureRmAccount.json .\Application_package
        Copy-Item .\ComparionMod.psm1 Application_package\
        Copy-Item .\ctt.exe .\Application_package\
        Copy-Item .\repo.txt Application_package\
        Copy-Item .\1_create_key.bat Application_package\ #CC added - per AD update
        Copy-Item .\2_import_key.bat Application_package\
        Copy-Item .\3_encrypt_config.bat Application_package\
        Copy-Item .\4_decrypt_config.bat Application_package\
        Copy-Item .\RemoveSelfHost.ps1 .\Application_package
        Copy-Item .\SelfHostSetup.ps1 .\Application_package
        Copy-Item .\SelfHostSetup.txt .\Application_package\README.TXT
        Copy-Item .\Sign.ps1 Application_package\
        Copy-Item .\SSLCertificates\signingcert.pfx Application_package\
#        Copy-Item .\TeamTimeAssistant\TeamTimeAssistantClient\mage.exe Application_package\
#        Copy-Item .\TeamTimeAssistant\TeamTimeAssistantClient\mageui.exe Application_package\
        Copy-Item .\uploadtoazure.ps1 .\Application_package
        Copy-Item .\uploadToS3.ps1 .\Application_package
        Copy-Item .\prepAzureIISserver.ps1 .\Application_package
        Copy-Item .\asmver.txt .\Application_package
#        Copy-Item -Recurse -Path .\TeamTimeAssistant\Certs Application_package\
        #Copy-Item .\TeamTimeAssistant\TeamTimeAssistantClient\signingcert.pfx Application_package\
        #remove install/config sub app files?
    }
}

if ($success) {
    Set-Location .\HTML5Comparion
    .\buildAnytime.ps1
    $success = $success -and $?
    Pop-Location
	Move-Item .\HTML5Comparion\*compile.txt .\

    cd .\Application_package

    $doZip=0
    if ($doZip) {
        Compress-Archive -CompressionLevel Fastest -Force -DestinationPath ./SelfHostSetup.zip -Path `
                            .\setup.ini,
                            .\mage.exe, 
                            .\1_create_key.bat,
                            .\2_import_key.bat,
                            .\3_encrypt_config.bat,
                            .\4_decrypt_config.bat,
                            .\mageUI.exe, 
                            .\ctt.exe, 
                            .\signingcert.pfx,
                            .\RemoveSelfHost.ps1,
                            .\uploadtoazure.ps1, 
                            .\SelfHostSetup.ps1,
                            .\Sign.ps1,
                            .\README.TXT,
                            .\ComparionMod.psm1, 
                            .\Application, 
                            .\HTML5Application, 
#                            .\TeamTimeClient,
                            .\Certs
    }

    cd ..

    $TargetPath = [io.path]::combine($XferPath, $Changeset) 
    if(!(Test-Path $TargetPath))
    {
        New-Item -Path $TargetPath -ItemType Directory -Force | Out-Null
    }

    if ($doZip) {
        Move-Item Application_package\SelfHostSetup.zip $TargetFile
    }

    $shtp = [io.path]::combine($TargetPath, "SelfHost")
    Move-Item -Path ./Application_package -Destination $TargetPath 

}


if ($success) {
    Write-Output -InputObject "Success"
    Exit 0
}
else {
    Write-Output -InputObject "Failed"
    Exit 1
}

# SIG # Begin signature block
# MIIfoQYJKoZIhvcNAQcCoIIfkjCCH44CAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUkWLr8btPmO5MCo4eu3r8EjZk
# r/mgghl7MIIEhDCCA2ygAwIBAgIQQhrylAmEGR9SCkvGJCanSzANBgkqhkiG9w0B
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
# CzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFPk1RGe5swsc4p/lkS9W
# rgTGtR+ZMA0GCSqGSIb3DQEBAQUABIICAGdMp9604blbztQlbwLxnvyJtwrTL/d1
# /Pby3W01Gdw6747mBpBH/TukQi7Q+WzYK4jBm+93eE28UjnL4oRRG0RosXprFv8r
# QowVdSaaaXid8d8+pjBw5iibR1cie776VAO/cfXx9KCSFoC2oa00sjaVKVWSd8z6
# z1SLSOp6ndJ3on/xgi8NwSwdRevuexzU8Vqg9PM73sMeQlGb5PoN4jUl5/ZTdZqM
# OuoSMCEahNGEPO//QetQoLFAyCvCH086pRWUywYNFpFSOMLgwDA+h6ELBkAg/GJt
# eg5GhEG8Alegyu6Eciq4CKJKwXAyvekBwAV58F8bDqZoY+W6ukAcugasEtuWpRWr
# BQellg2swckIb0MbobdjmVcG7wj+AxQLEvbn/1I432S7Axg4l+yJFIe0JErI3wQM
# MjhxdxjC/fpDC1DbJhNhztYdMkiYxKH/inBYY3dtZV8wbr2hpuWnQbqF2iJpi3Jr
# gm+jYofnAkFsOHTKlXv6GVtbmIyokpfFhJObZJQGCvRFkw8No/qdSf1H334ZMe9e
# 37lCOl5ISg6/hnwTxme1bEvV/n9bzcU/hwrxPhNNuQY/8jpeERIaxFB6zkwqw1H/
# BFtdTSiRT//WCgy2I2phIqWikGTVHgSVs67nRxJoWjSGd4aAwLEiNZOXo1RiOMoC
# v0u3rJ+6I/8roYICKDCCAiQGCSqGSIb3DQEJBjGCAhUwggIRAgEBMIGOMHoxCzAJ
# BgNVBAYTAkdCMRswGQYDVQQIExJHcmVhdGVyIE1hbmNoZXN0ZXIxEDAOBgNVBAcT
# B1NhbGZvcmQxGjAYBgNVBAoTEUNPTU9ETyBDQSBMaW1pdGVkMSAwHgYDVQQDExdD
# T01PRE8gVGltZSBTdGFtcGluZyBDQQIQK3PbdGMRTFpbMkryMFdySTAJBgUrDgMC
# GgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcN
# MTkwODA2MTIxODA2WjAjBgkqhkiG9w0BCQQxFgQUXYmdjbHW5wOwrD5I9ddKt7bt
# K2EwDQYJKoZIhvcNAQEBBQAEggEAglCRRcpb5RKBHcThTaGN2DIIigWEyAv4Ens8
# r8++7BepHeRKBPCfrw1j+IsfGZyT6eXVemPE3hYGpI5xANSGUcSnrjF0nFJejTSK
# UxD4bGN2rtflJhNjuDw1ba4L4efJ2UzFV4eY6jgruB3nUV+YstyO5Iw/WtzHZO/U
# 3UeSqTAcFywiCNP/XXpqR8yASkc2M10/UCbhdgU3skoQrpRXPJLnHsCmy2wu8dIW
# v1h2tpolX87GVrNeoFDo6quNvHcw4DAeaHt7yTwsOHy/T+7fuz5vkOo8ekel+N5F
# AOo704kDoaqmiLhc7FvsZUEoqO+IsBxEFo6Hwjm7cLu4wLvUcg==
# SIG # End signature block
