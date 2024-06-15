### 帮助文档

在`Debug`模式下运行项目查看帮助文档,
站点会将文档生成到`_Statics`文件夹下,该文件下生成的文件可以拷贝用于静态部署~~

```bash
dotnet run
```

请注意如果程序集有变更可以取消`Program.cs`中的注释,重新生成API Schema文件

```csharp
//当变更的时候生成yaml文件:
//await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("seed/docfx.json", options);
```