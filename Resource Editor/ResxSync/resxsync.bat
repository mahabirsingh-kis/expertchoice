@ECHO OFF
FOR %%i IN (*.resx) DO IF NOT "%%i"=="English.resx" resxsync.exe English.resx %%i /v > %%i.log
