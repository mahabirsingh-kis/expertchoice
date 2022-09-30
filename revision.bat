@ECHO OFF
WHERE hg
IF %ERRORLEVEL% NEQ 0 GOTO LBL_NO_HG

SET fn="%1"
IF %fn% EQU "" SET fn="Application/revision.txt"

hg identify --num > %fn%
ECHO | SET /p="Parent revision: " >> %fn%
hg identify --num >> %fn%
ECHO | SET /p="Parent ID: " >> %fn%
hg identify --id >> %fn%
ECHO | SET /p="Branch: " >> %fn%
hg identify --branch >> %fn%
ECHO Pre-build local date: %DATE% %TIME:~0,8% >> %fn%

:LBL_NO_HG
