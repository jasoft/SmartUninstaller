# SmartUninstaller

一款创新的Windows卸载工具，具有智能便携软件管理、AI驱动的残留识别、损坏软件救援模式等核心特性。

## 核心特性

- **智能便携软件管理**: 自动检测和管理便携软件
- **AI驱动的残留识别**: 使用ML.NET机器学习技术准确识别软件残留
- **损坏软件救援模式**: 专门处理损坏的软件卸载
- **轻量级高性能**: 安装包<5MB，内存占用<20MB，启动时间<2秒
- **透明开源**: 核心算法开源，提供详细卸载日志

## 技术栈

- C# + .NET 10.0
- WPF (Windows Presentation Foundation)
- ML.NET (机器学习)
- CommunityToolkit.Mvvm (MVVM框架)
- xUnit + Moq (单元测试)

## 项目结构

```
SmartUninstaller/
├── src/
│   ├── SmartUninstaller.Core/      # 核心业务逻辑
│   ├── SmartUninstaller.UI/        # WPF用户界面
│   ├── SmartUninstaller.AI/        # AI引擎
│   ├── SmartUninstaller.Data/      # 数据访问
│   └── SmartUninstaller.Shared/    # 共享组件
└── tests/                          # 测试项目
```

## 构建

```bash
dotnet build
dotnet test
```

## 许可证

MIT License
