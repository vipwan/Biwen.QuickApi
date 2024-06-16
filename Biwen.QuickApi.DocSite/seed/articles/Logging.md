日志模块
=====================

提供Serilog日志模块,可以通过配置文件配置日志输出,支持控制台、文件、数据库等多种输出方式。<br/>
请引用`Biwen.QuickApi.Logging.Serilog`库,并在`Startup.cs`中调用`UseSerilogFromConfiguration`方法,如下所示:

```csharp
using Biwen.QuickApi.Logging;

//清空默认日志提供程序
builder.Logging.ClearProviders();
//使用配置文件Serilog注册
builder.Host.UseSerilogFromConfiguration();

```

日志配置
---------------------

Serilog日志配置文件示例:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "{Timestamp:HH:mm:ss}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Verbose"
        }
      }
    ]
  }
}
```
API文档
---------------------

相关API文档:
[Serilog](https://github.com/serilog/serilog) &nbsp;
[Serilog Configuration](https://github.com/serilog/serilog-settings-configuration)