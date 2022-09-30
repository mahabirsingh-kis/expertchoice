rem @echo off
setlocal

set /p password=Cert password 

set HostName=
set CertName=
	
if [%HostName%] == [] set HostName=localhost
if [%CertName%] == [] set CertName=localhostTTACert

set CAName=%HostName%RootCA

call ImportRoot %CAName%
rem call ImportCertificate %CertName% %password%

:eof
@echo on

