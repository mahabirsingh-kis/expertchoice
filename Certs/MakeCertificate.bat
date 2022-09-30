rem @echo off
setlocal

if [%1] == [] goto eof
if [%2] == [] goto eof

certmgr -del -c -n "%1" -s -r localMachine My 

makecert -sk %1 -n "CN=%1" "%1.cer" -iv "%2.pvk"  -ic "%2.cer" -sr localmachine -ss my -sky exchange -pe

:eof
@echo on