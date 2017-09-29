@ECHO OFF
REM This is here because MSBuild integration in VS is currently broken when SDK 2.0 tools are installed.
"../../packages/docfx.console.2.24.0/tools/docfx.exe"

REM @docfx metadata -f
REM @docfx build -f docfx.json