@echo off
cd server

:start
python -m flask --app server run

set "reply=y"
set /p "reply=Do you want to rerun the server? [y|n]: "
if /i "%reply%" == "y" goto :start