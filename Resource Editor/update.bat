@echo off
set origdir=%CD%
pushd "%~dp0"
set REL_PATH=..\
set ABS_PATH=
rem // save current directory
pushd .
rem // change to relative directory and save value of CD (current directory) variable
cd %REL_PATH%
set ABS_PATH=%CD%
rem // restore current directory
popd

SET ProgFiles86Root=%ProgramFiles(x86)%
IF EXIST "%ProgFiles86Root%" GOTO amd64
SET ProgFiles86Root=%ProgramFiles%
:amd64

SET VSTOOLS=%ProgFiles86Root%\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools

:STARTUPDATE 

cd %ABS_PATH%\Silverlight\Resources

"%VSTOOLS%"\resgen Strings.resx /publicClass /str:VB,Infrastructure,,Strings.Designer.vb.tmp
%ABS_PATH%\sed\sed "s/Namespace Infrastructure/Namespace My.Resources/" Strings.Designer.vb.tmp > Strings.Designer.vb
del *.tmp
del Strings.resources

set BUILD_STATUS=%ERRORLEVEL%
if NOT [%BUILD_STATUS%]==[0] goto eof

:eof

cd %origdir%
