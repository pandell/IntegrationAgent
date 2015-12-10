@echo off

rem NOTE: Copy of https://github.com/pandell/node-repo-init/blob/v3.0.0/sync/tools/Run.cmd

rem |  Common pattern of MSBUILD target invocation
rem |  for Pandell builds.
rem |
rem |  Syntax:
rem |  run.cmd <targets> <configurations-or-x> <project-or-x> <pause-or-x> [extraParam1] ... [extraParam5]
rem |
rem |  <targets>: list of semicolon-separated targets to build
rem |      (e.g. "Clean;Build;Deploy").
rem |  <configurations>: list of plus-separated configurations
rem |      to build (e.g. "Debug+Release").
rem |      Note: if requested targets don't care about configurations
rem |      (e.g. clobber), it is recommended that you use "x"
rem |      as configurations value.
rem |  <project-or-x>: when not "x", must be path to a valid
rem |      MSBuild project file, which will be used to run the
rem |      requested targets; when "x", last "*.proj" file in
rem |      project's root directory will be used to run
rem |      the requested targets.
rem |  <pause-or-x>: when specified ("pause"), execution will
rem |      pause just before this batch finishes - the user must
rem |      hit ENTER key to continue; when not "pause" (case-insensitive),
rem |      the batch will finish without waiting for user input.
rem |      Note: if IGNORE_PAUSE environment variable is defined,
rem |      pause specification will be ignored and batch will
rem |      finish without waiting for user input.
rem |
rem |  NOTE: Run.cmd expects to be run on 64bit Windows installation. 32bit
rem |      installations are not supported. Because of this, Run always
rem |      starts 64bit MSBUILD process. However, sometimes -- because of build
rem |      system limitations -- 32bit MSBUILD is needed (Silverlight projects,
rem |      most notably). Switching to 32bit mode is supported via "file flag".
rem |      If a file "Run.cmd.x86" (content is ignored, zero-length-file is ok)
rem |      exists in the same directory as this "Run.cmd" script, 32bit MSBUILD
rem |      will be started instead of the 64bit one.

setlocal

rem - find (the last) *.proj file in root directory
set ProjFile=%~3
if /I not "%ProjFile%"=="x" goto :foundProjFile
for %%f in ("%~dp0..\*.proj") do set ProjFile=%%f
if /I not "%ProjFile%"=="x" goto :foundProjFile
echo ERROR: Can't find project file
endlocal
exit /B 1

:foundProjFile

rem - Add paths where MSBuild.exe can be found to PATH
rem - (this allows us to look in multiple locations,
rem - not just hard-code one)
set MsbuildPath=%ProgramFiles(x86)%\MSBuild\14.0\Bin\amd64;%ProgramFiles(x86)%\MSBuild\12.0\Bin\amd64;%systemroot%\Microsoft.NET\Framework64\v4.0.30319
if exist "%~dp0Run.cmd.x86" set MsbuildPath=%ProgramFiles(x86)%\MSBuild\14.0\Bin;%ProgramFiles(x86)%\MSBuild\12.0\Bin;%systemroot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%MsbuildPath%;%PATH%

rem - Run MSBuild (echoing the command itself,
rem - to help with troubleshooting)
@echo on
msbuild /nologo /nodeReuse:false /maxcpucount /consoleloggerparameters:ShowCommandLine;ShowTimestamp "/target:%~1" "/property:Configurations=%~2" "%ProjFile%" %5 %6 %7 %8 %9
@echo off
endlocal

rem - Pause (if requested and allowed) so that
rem - the user can review build success/failure
if /I "%~4"=="pause" (
    if not defined IGNORE_PAUSE pause
)
