### 帮助文档

直接运行项目查看帮助文档

```bash
dotnet run
```

请注意如果程序集有变更可以取消`Program.cs`中的注释,重新生成API Schema文件

```csharp
//当变更的时候生成yaml文件:
//await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("seed/docfx.json", options);
```