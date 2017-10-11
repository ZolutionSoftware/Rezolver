@ECHO OFF
REM This is here because MSBuild integration in VS is currently broken when SDK 2.0 tools are installed.
REM YOU MIGHT NEED TO set PATH=%PATH%;"e:\github\rezolver\packages\docfx.console.2.24.0/tools"
pushd ..\..\packages\docfx.console.2.24.0\tools
set PATH=%PATH%;%CD%
popd
docfx metadata -f
docfx build -f docfx.json