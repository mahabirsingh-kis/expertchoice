@ECHO OFF

if "%GUROBI_HOME%"=="" (
  echo.
  echo Gurobi installer changes have not taken effect yet.
  echo Please restart your machine before continuing.
  echo.
  set /p JUNK= [Hit ENTER to exit]
  exit
)

cd "%GUROBI_HOME%

if exist build (
  rmdir /s /q build
)

set pythonpath=c:\Python27

set /P pythonpath="Python installation directory (hit ENTER to use %pythonpath%): "
echo.

%pythonpath%\python setup.py install

echo.
set /p JUNK= [Hit ENTER to exit]
