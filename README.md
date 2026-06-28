# SmartUninstaller

一款创新的 Windows 卸载工具，具有智能便携软件管理、AI 驱动的残留识别、损坏软件救援模式等核心特性。

## 🚀 快速开始

### 方式一：双击运行（最简单）

```
双击 run.bat
```

如果有已构建版本会直接启动，否则会自动编译后启动。

### 方式二：命令行运行

```powershell
# 已有发布版本时直接运行
.\publish\SmartUninstaller.UI.exe

# 或使用 dotnet run
dotnet run --project src\SmartUninstaller.UI\SmartUninstaller.UI.csproj
```

### 方式三：先构建再运行

```powershell
# 构建发布版
dotnet publish src\SmartUninstaller.UI\SmartUninstaller.UI.csproj -c Release -o publish

# 运行
.\publish\SmartUninstaller.UI.exe
```

## 📋 环境要求

- **Windows 10/11**
- **.NET 8.0 SDK**（下载：https://dotnet.microsoft.com/download/dotnet/8.0）
- **Visual Studio 2022**（可选，用于开发调试）

## 🏗️ 项目结构

```
SmartUninstaller/
├── src/
│   ├── SmartUninstaller.Core/      # 核心业务逻辑（引擎 + 服务 + 接口 + 模型）
│   ├── SmartUninstaller.UI/        # WPF 用户界面（8个视图 + 8个ViewModel）
│   ├── SmartUninstaller.AI/        # AI 引擎（ML.NET 残留识别）
│   ├── SmartUninstaller.Data/      # 数据层（SQLite EF Core）
│   └── SmartUninstaller.Shared/    # 共享组件（常量/枚举/扩展/工具）
├── tests/                          # 单元测试
├── run.bat                         # 一键运行脚本
└── README.md
```

## ✨ 核心功能

| 功能 | 说明 |
|------|------|
| **已安装软件管理** | 扫描注册表/文件系统/WMI，DataGrid 列表展示，支持搜索、批量卸载 |
| **便携软件管理** | 自动检测便携软件（特征文件/目录分析），支持一键删除 |
| **残留清理** | 扫描已卸载软件的残留（目录/注册表/临时文件/开始菜单快捷方式） |
| **浏览器扩展** | 扫描 Chrome/Edge/Firefox 扩展，支持按浏览器筛选 |
| **Windows 应用** | 通过 PowerShell 扫描 UWP/MSIX 应用，支持卸载 |
| **系统清理** | 分析临时文件/回收站/浏览器缓存/系统日志，一键清理 |
| **备份管理** | 备份软件数据到本地，支持恢复和删除 |
| **SQLite 历史** | 所有卸载操作自动记录到 SQLite 数据库 |
| **AI 残留识别** | ML.NET 机器学习模型预测残留文件 |

## 🔧 技术栈

- **C# + .NET 8.0**
- **WPF** (Windows Presentation Foundation)
- **CommunityToolkit.Mvvm** (MVVM 框架)
- **ML.NET** (机器学习)
- **Entity Framework Core + SQLite** (数据持久化)
- **xUnit + Moq** (单元测试)

## 📄 许可证

MIT License
