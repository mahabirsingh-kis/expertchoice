rem @echo off
setlocal

if [%1] == [] goto eof

certmgr -del -c -n "%1" -s -r localMachine root 
certmgr -del -crl -n "%1" -s -r localMachine root 

makecert -n "CN=%1" -r -sv "%1.pvk" "%1.cer" 
makecert -crl -n "CN=%1" -r -sv "%1.pvk" "%1.crl"

certmgr -del -c -n "%1" -s -r localMachine root 
certmgr -del -crl -n "%1" -s -r localMachine root 

certmgr -add -c "%1.cer" -s -r localMachine root 
certmgr -add -crl "%1.crl" -s -r localMachine root 

:eof
@echo on