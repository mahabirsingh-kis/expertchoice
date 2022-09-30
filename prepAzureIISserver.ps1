 param (
    [string]$loadIni="azure.publish.ini",
    [string]$Websitename="foo",
    [string]$DBName="CoreDB"
    )

#run the following to allow powershell remoting
#Set-Item WSMan:\localhost\Client\TrustedHosts -Value "*" -Force
#Get-Item WSMan:\localhost\Client\TrustedHosts

Clear-Host
Push-Location (Split-Path $MyInvocation.MyCommand.Path)

Import-Module -Force .\ComparionMod.psm1
Remove-Item .\compile.txt -ErrorAction SilentlyContinue
$setupFile = 'setup.ini' 

if ($loadIni -ne ""){
    if (!(Test-Path $loadIni)){
        Write-Output $loadIni" not found"
        Exit 1
    }
    $success = $?
} else { 
    if (!$success) {
        Write-Output "setup.ini not found"
        Exit 1
    }
} 

Copy-Item $loadIni $setupFile

(Get-Content $setupFile) | Foreach-Object {$_ -replace 'azure-site-name',$WebsiteName} | Out-File $setupFile
(Get-Content $setupFile) | Foreach-Object {$_ -replace 'azure-db-name',$DBname} | Out-File $setupFile

$success=1
$Xfer = Get-INIContent ([io.path]::combine(${env:userprofile}, 'XferPath.ini'))
$SETUP = Get-INIContent ($setupFile)
$TargetPath = ([io.path]::combine($Xfer.Paths.XferPath, $SETUP.info.INFO_Changeset))

$IISWebTarget = [io.path]::combine($Xfer.remote.IISWebDir, $SETUP.setup.DNS_FQDN)
$pw = convertto-securestring -AsPlainText -Force -String $Xfer.remote.Password
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist ("{0}\{1}" -f $Xfer.remote.MachineName, $Xfer.remote.UserName), $pw
$IISWebShare = "\\" + $Xfer.remote.MachineName + "\" +  $Xfer.remote.IISWebShare
$staging= [io.path]::combine($Xfer.remote.IISWebDir, "staging")
$buildSource = ([io.path]::combine($xfer.paths.XferPath, $SETUP.info.INFO_Changeset,"Application_package"))
$stageDir = [io.path]::combine($Xfer.remote.IISWebDir, "staging")
$stageShare = [io.path]::combine($IISWebShare, "staging")
$stageTarget = [io.path]::combine($IISWebShare, $SETUP.setup.DNS_FQDN)

$session = New-PSSession -ComputerName $Xfer.remote.MachineName -Credential $cred
    
try
{
    $siteExists = Invoke-Command -Session $session -ScriptBlock { param($dir) 
        Test-Path $dir 
    } -ArgumentList $IISWebTarget

    $x = Invoke-Command -Session $session -ScriptBlock { param($staging) 
        cd ..
        try
        {
            $empty = ([io.path]::combine($staging, 'empty'))
            New-Item -ItemType Directory -Path $empty
            robocopy.exe $empty $staging /mir
            Remove-Item -Recurse $empty -Force
            Remove-Item -Recurse $staging -Force
        } catch{
        }
             
    } -ArgumentList $stageDir

    $startTime = Get-Date
    #Copy-Item -Recurse $buildSource $stageShare
    Robocopy.exe $buildSource $stageShare /S /E /ZB /R:5 /W:5 /TBD /NP /V /MT:16
    $finishTime = Get-Date
    Write-Output (' Total time used (minutes): {0}`r`n' -f ($finishTime - $startTime).TotalMinutes)

    if($siteExists)
    {
        $x =  Invoke-Command -Session $session -ScriptBlock { param($dir) 
            cd $dir
            ./RemoveSelfHost.ps1
            cd ..
        } -ArgumentList $IISWebTarget
        $empty = ([io.path]::combine($staging, 'empty'))
        New-Item -ItemType Directory -Path $empty
        robocopy.exe $empty $stageTarget /mir
        Remove-Item -Recurse $empty -Force
        Remove-Item -Recurse $stageTarget -Force
        #Remove-Item $stageTarget -Recurse -Force
    }

    $startTime = Get-Date
    Rename-Item -Force $stageShare $stageTarget
    $finishTime = Get-Date
    Write-Output (' Total time used (minutes): {0}`r`n' -f ($finishTime - $startTime).TotalMinutes)

    $x =  Invoke-Command -Session $session -ScriptBlock { param($dir)
        Write-Output "SelfHostSetup`r`n"
        Write-Output ($dir + "`r`n")
        cd $dir
        ./SelfHostSetup.ps1
    } -ArgumentList $IISWebTarget
}
catch
{
}
finally
{
    Get-PSSession | Remove-PSSession
    #Remove-PSSession $session
}

$success = $?
if ($success) {
    Write-Output -InputObject "Success`r`n"
    Exit 0
}
else {
    Write-Output -InputObject "Failed`r`n"
    Exit 1
}

# SIG # Begin signature block
# MIIaZgYJKoZIhvcNAQcCoIIaVzCCGlMCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU45QmPO50dqjU+KGgQjL5ail9
# hfOgghQlMIIEfTCCA2WgAwIBAgIDG+cVMA0GCSqGSIb3DQEBCwUAMGMxCzAJBgNV
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
# hkiG9w0BCQQxFgQUbwb5jO9E5APcvBbL9YLqnRumcyMwDQYJKoZIhvcNAQEBBQAE
# ggIAPdea32X5Jvg3ACbfTxeHhuTgOVQIuCx5GOcv1k5gh1oaGQTJQbfce19c//CW
# wcy97nOUaUUJKYFkWcb5JQj2QZBDwZcllhiDuyMGXVIIhTRs1hZIawM7LOQphp9R
# gCNXWdxQwyRXid1AI+8jtTl2L5NcM7lHs+E8ApTRot4I5w+G062C5oOtxfZMR1Ik
# F/cY+YeNc6f/35fxAQzmpksgPyECLkOnCGH0h2FCRx5YZQccFTpb9U64pj2sEmm8
# cjw481JIkZwD3hhWM3iVa645LwKaSqrOXmRJrqC+iJPGi8u5oYbkV8z7sEyTrHUG
# zlrFeGVs1kdOucI5hdMzu7fsjqvSIBxlj4AUMqkXaUVZwWRg2k+fiK+JDQIfKGG0
# W2zNhUA7xdrckNh+ZeYrDTPBlh3NQMMp+SUZM5DVeRW9NrwugXxrNQshhkqHRLqu
# E7rJTrk25+xa8oHSLb4G2FcZknHslT+PCyMmYuS+7UwVmjqhuRx6H5UQ+DBMo89i
# 8ZZjGVSfPUlfIfLxcUC4YqoEV0zuhJeizY0sX3NNpmW+FI5+odxoms0u6J4KkLyH
# ml5kqDdO26WriksPl4RReVlv6nrhkfhAbX0NqT3ZEI7gxCIwGgDEuh7go4427gWd
# Qw604uLUX+FIzYQ2j67DnLGFyGJTXyeiU3FlRSXkpBJEeW+hggJDMIICPwYJKoZI
# hvcNAQkGMYICMDCCAiwCAQEwgakwgZUxCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJV
# VDEXMBUGA1UEBxMOU2FsdCBMYWtlIENpdHkxHjAcBgNVBAoTFVRoZSBVU0VSVFJV
# U1QgTmV0d29yazEhMB8GA1UECxMYaHR0cDovL3d3dy51c2VydHJ1c3QuY29tMR0w
# GwYDVQQDExRVVE4tVVNFUkZpcnN0LU9iamVjdAIPFojwOSVeY45pFDkH5jMLMAkG
# BSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJ
# BTEPFw0xOTAxMTgxODQxMjZaMCMGCSqGSIb3DQEJBDEWBBTb3C8WMVTNju5Sl8yh
# BhJ8r/mvsTANBgkqhkiG9w0BAQEFAASCAQCy/FFj8pKL0OrOTGUyxoSMggo5clDy
# 3KUKJkIh0GGvj9rFCBG2v2zJoyJbadw1tcMLhAco7945XTXXI3DzFecEwMvBwAcw
# cSXImS7A43ls+eMUtp6VyyLH0/BUFja9d2oeMo2rjoP2Qfq849YsY2lUOlnLl6Ee
# J61iVMN+VwhAHStU6o5WsxcuWVn2BOqSg9q6piSS+QhgHxyuZBCngN0AdtFlQzBr
# enWqAXBnau43frSL/h6MQZF8x0AXsSRGh9Hum4d4HN+k8LplvAMK+Y9QVw4YPc5t
# QOrvlX7B3x5bQKcTVmhB81SrrFxoz9yxH8bV6So5CcqAYHitMmYEt7bJ
# SIG # End signature block
