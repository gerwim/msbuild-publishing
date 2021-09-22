@ECHO OFF

IF [%1] == [] (
    ECHO Please run this script with the version parameter, e.g. "build.bat 1.0.0"
    EXIT /b 1
) else (
    SET PACKAGE_VERSION=%1
)

powershell -ExecutionPolicy Bypass -command "& { Import-Module '%~dp0\build\psake\psake.psd1'; Invoke-psake '%~dp0\build\default.ps1' -taskList Test,Pack -parameters @{packageVersion='%PACKAGE_VERSION%'} }"