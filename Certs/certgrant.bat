REM Install the certificate created on another machine using MMC, then run this batch file to grant the permissions.
REM certgrant FarmServiceIISCert
winhttpcertcfg -g -c LOCAL_MACHINE\My -s %1 -a NETWORKSERVICE
