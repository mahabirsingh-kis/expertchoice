@echo off
set VersionNum=%1
call "%programfiles%\Altiris\Wise\Windows Installer Editor\wfwi.exe" Core.wsi /c /s /p ProductVersion=%VersionNum%
call "%programfiles%\Altiris\Wise\WiseScript Package Editor\Wise32.exe" Core.wse /c /d_VERSION_=%VersionNum%
del %VersionNum%.exe 2>NUL
copy Core.wsi %VersionNum%.wsi >NUL
rename core.exe %VersionNum%.exe 2>NUL 
