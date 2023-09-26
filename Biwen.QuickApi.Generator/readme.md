[如何调试](https://github.com/JoanComasFdz/dotnet-how-to-debug-source-generator-vs2022#solution-structure)


## QuickApi源代码生成器

### 介绍

- 提供预生成IEndpointRouteBuilder的扩展方法,用于快速注册Api.用于显著提升性能和开发效率.

### 说明
- 当前还处于开发阶段,不建议在生产环境中使用.
- 由于1.0版本使用Emit和动态类型会导致性能上的损失.所以2.0版本提供源代码生成器的方式生成强类型代码.



### 参考文档
- https://learn.microsoft.com/zh-cn/dotnet/csharp/roslyn-sdk/source-generators-overview
- https://github.com/dotnet/roslyn-sdk/tree/main/samples/CSharp/SourceGenerators
