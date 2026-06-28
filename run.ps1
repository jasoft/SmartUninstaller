# SmartUninstaller 一键运行脚本
# 用法: 右键 -> 使用 PowerShell 运行

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SmartUninstaller - Windows 智能卸载工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 方式1: 如果已有发布版本，直接运行
$publishExe = Join-Path $root "publish\SmartUninstaller.UI.exe"
if (Test-Path $publishExe) {
    Write-Host "[OK] 发现已构建的程序，直接启动..." -ForegroundColor Green
    Start-Process -FilePath $publishExe
    Write-Host "应用已启动！" -ForegroundColor Green
    exit 0
}

# 方式2: 需要先构建
Write-Host "[INFO] 未找到发布版本，开始构建..." -ForegroundColor Yellow

# 检查 .NET SDK
$dotnetVersion = & dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "[ERROR] 未安装 .NET SDK！请先安装 .NET 8.0 SDK" -ForegroundColor Red
    Write-Host "下载地址: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Read-Host "按回车键退出"
    exit 1
}
Write-Host "[OK] .NET SDK 版本: $dotnetVersion" -ForegroundColor Green

# 构建
Write-Host ""
Write-Host "[BUILD] 正在构建项目..." -ForegroundColor Yellow
& dotnet publish "$root\src\SmartUninstaller.UI\SmartUninstaller.UI.csproj" -c Release -o "$root\publish" --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] 构建失败！" -ForegroundColor Red
    Read-Host "按回车键退出"
    exit 1
}
Write-Host "[OK] 构建成功！" -ForegroundColor Green

# 运行
Write-Host "[RUN] 正在启动应用..." -ForegroundColor Yellow
Start-Process -FilePath $publishExe
Write-Host ""
Write-Host "SmartUninstaller 已启动！" -ForegroundColor Green
