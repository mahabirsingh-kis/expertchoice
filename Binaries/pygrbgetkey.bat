@ECHO OFF
gurobi.bat -c "from grb_common import grbgetkey;grbgetkey()" %*
