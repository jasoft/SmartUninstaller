@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo   SmartUninstaller - Windows ??????
echo ========================================
echo.

REM ??????????
if exist "publish\SmartUninstaller.UI.exe" (
    echo [OK] ????...
    start "" "publish\SmartUninstaller.UI.exe"
    goto :end
)

REM ??dotnet
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] ??? .NET SDK?
    echo ???? .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    goto :end
)

REM ??
echo [BUILD] ??????????...
dotnet publish "src\SmartUninstaller.UI\SmartUninstaller.UI.csproj" -c Release -o "publish" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] ?????
    pause
    goto :end
)

echo [OK] ?????
echo [RUN] ????...
start "" "publish\SmartUninstaller.UI.exe"

:end
