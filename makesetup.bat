@echo off
call build.bat /setup /obfuscate /quiet
cd setup
call package %ECBUILDNUM%
cd .. 
