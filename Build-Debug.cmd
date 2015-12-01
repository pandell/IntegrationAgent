@echo off

call "%~dp0tools\Build-Debug-Server" || goto :error

goto :done

:error
echo Build failed with error %errorlevel%.

:done
if not defined IGNORE_PAUSE pause
exit /b %errorlevel%
