@echo off

call "%~dp0tools\Run" UnitTest Debug x x || goto :error

goto :done

:error
echo Test failed with error %errorlevel%.

:done
if not defined IGNORE_PAUSE pause
exit /b %errorlevel%
