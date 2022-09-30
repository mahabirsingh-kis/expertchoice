rem @echo off
setlocal

set HostName=%1
set CertName=%2

if [%HostName%] == [] set HostName=%computername%
if [%CertName%] == [] set CertName=%HostName%Cert

set CAName=%HostName%RootCA

call MakeRoot %CAName% %buildonly%
call MakeCertificate %CertName% %CAName% %buildonly%
call grant %CertName%
call CertDecoder %CertName%.cer %CertName%.txt
copy %CertName%.txt ..\MachineCert.txt

:eof
@echo on