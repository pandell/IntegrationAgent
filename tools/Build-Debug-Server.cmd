@echo off

call "%~dp0Run" Build Debug x x || goto :error

goto :done

:error
echo Server build failed with error %errorlevel%.

:done
exit /b %errorlevel%
