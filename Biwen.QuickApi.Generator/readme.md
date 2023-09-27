
## QuickApi源代码生成器

### 介绍
- Biwen.QuickApi.SourceGenerator 1.0.0已经发布欢迎使用.
- 提供预生成IEndpointRouteBuilder的扩展方法,用于快速注册Api.用于显著提升性能和开发效率.

### 说明
- 
- 由于1.0版本使用Emit和动态类型会导致性能上的损失.所以提供SourceGenerator源代码生成器的方式生成强类型代码.

### Enjoy!

- 1.安装Biwen.QuickApi.SourceGenerator 1.1.0
- <del>2.全局引用你[QuickApi]特性标注的地方 比如: global using Biwen.QuickApi.DemoWeb.Apis;</del>
- 3.调用 app.MapGenQuickApis("api"); 注册所有Api,尽情享用吧!

### 参考文档
- https://learn.microsoft.com/zh-cn/dotnet/csharp/roslyn-sdk/source-generators-overview
- https://github.com/dotnet/roslyn-sdk/tree/main/samples/CSharp/SourceGenerators

- https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#out-of-scope-designs



### 如何调试Generator
- [如何调试](https://github.com/JoanComasFdz/dotnet-how-to-debug-source-generator-vs2022#solution-structure)


