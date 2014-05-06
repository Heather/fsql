@echo off

pushd .
cd .\win
set ABS_PATH=%CD%
"%ABS_PATH%\fsql.bat"
popd