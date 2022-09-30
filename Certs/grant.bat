rem @echo off
setlocal
if [%1] == [] goto eof

del keyfile.dat > nul

findprivatekey My LocalMachine -n "CN=%1" -a > keyfile.dat

for /f "delims=" %%f in (keyfile.dat) do cacls "%%f" /E /G "Everyone":R 

:eof
@echo on