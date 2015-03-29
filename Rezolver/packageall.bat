@echo off

cd rezolver
call package.bat

cd ..\rezolver.configuration
call package.bat

cd ..\rezolver.configuration.json
call package.bat

cd ..\rezolver.mvc5
call package.bat

cd ..\rezolver.commonservicelocator
call package.bat

cd..