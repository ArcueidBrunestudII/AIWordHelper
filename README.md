# AI划词助手

一个低后台占用的Windows托盘程序，实现划词后自动调用AI进行翻译/解释等功能。

## 环境要求

- Windows 10/11
- .NET 8 SDK (下载: https://dotnet.microsoft.com/download/dotnet/8.0)

## 编译运行

```bash
cd AIWordHelper
dotnet build
dotnet run
```

或者发布为单文件：

```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## 使用说明

1. 程序启动后会在系统托盘显示图标
2. 右键托盘图标 → "控制台" 打开设置
3. 配置 API URL 和 Key（支持 OpenAI 兼容格式）
4. 点击"刷新模型"获取可用模型列表
5. 添加/启用提示词模板
6. 在任意程序中选中文字，等待设定时间后会弹出AI回复窗口

## 功能特性

- 系统托盘运行，低资源占用
- 支持 OpenAI 兼容 API（OpenAI、Claude API、Ollama 等）
- 可自定义等待时间（0.5-10秒）
- 支持多个提示词模板，可复选启用
- AI回复窗口支持拖动、选中文字、右键关闭
- 流式输出，实时显示AI回复

## 项目结构

```
AIWordHelper/
├── Models/
│   ├── AppConfig.cs          # 配置模型
│   └── PromptTemplate.cs     # 提示词模板
├── Services/
│   ├── AIService.cs          # AI API调用
│   ├── ConfigService.cs      # 配置读写
│   ├── ClipboardService.cs   # 剪贴板操作
│   └── MouseHookService.cs   # 全局鼠标钩子
├── Views/
│   ├── SettingsWindow.xaml   # 设置窗口
│   └── AIResponseWindow.xaml # AI回复窗口
├── App.xaml                  # 应用入口
└── Resources/
    └── tray_icon.ico         # 托盘图标
```

## 注意事项

- 首次运行需要配置 API URL 和 Key
- 托盘图标如果不存在会使用系统默认图标
- 配置文件保存在程序目录下的 config.json
