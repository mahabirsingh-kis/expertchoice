rem @echo off
setlocal
if [%1] == [] goto eof
if [%2] == [] goto eof

certmgr -del -c -n "%1" -s -r localMachine My 
rem certmgr -add -c "%1.cer" -s -r localMachine My
winhttpcertcfg -i "%1.pfx" -c LOCAL_MACHINE\My -a "NETWORK SERVICE" -p %2

:eof
@echo on