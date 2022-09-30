@echo off
set orig=%1
set str=%~2 %~3 %~4 %~5 %~6 %~7
set str=%str%##
set str=%str:                ##=##%
set str=%str:        ##=##%
set str=%str:    ##=##%
set str=%str:  ##=##%
set str=%str: ##=##%
set replace=%str:##=%

echo replacing %orig% with %replace%
for /R . %%G IN (*.config) DO (
del %%G.bak 2>NUL
sed\sed -i.bak "s¥%orig%¥%replace%¥" %%G >NUL
del %%G.bak 2>NUL
del sed*.*
)
