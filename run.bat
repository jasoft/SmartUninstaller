@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo   SmartUninstaller - Windows ??????
echo ========================================
echo.

REM ?? windir ??????? Windows 11 ????????? WPF ???
if "%windir%"=="" set windir=C:\WINDOWS

if exist "publish\SmartUninstaller.UI.exe" (
    echo [OK] ????...
    start "" "publish\SmartUninstaller.UI.exe"
    goto :end
)

dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] ??? .NET 9 SDK
    pause
    goto :end
)

echo [BUILD] ???...
if exist "publish" rmdir /s /q "publish"
dotnet publish "src\SmartUninstaller.UI\SmartUninstaller.UI.csproj" -c Release -o "publish" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] ????
    pause
    goto :end
)

echo [OK] ?????????...
start "" "publish\SmartUninstaller.UI.exe"

:end
