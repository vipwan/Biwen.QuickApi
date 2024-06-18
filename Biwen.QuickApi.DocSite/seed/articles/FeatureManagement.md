### Biwen.QuickApi.FeatureManagement

#### 安装Nuget包

```shell
dotnet add package Biwen.QuickApi.FeatureManagement --version 2.0.0+
```

在配置文件中添加FeatureManagement配置

```json
{
  "$schema":"https://raw.githubusercontent.com/microsoft/FeatureManagement-Dotnet/main/schemas/FeatureManagement.Dotnet.v1.0.0.schema.json#",

  "FeatureManagement": {
	"myfeature": true
  }
}
```

#### 在MinimalApi中使用FeatureManagement

```csharp
app.MapGet("feature-test", () =>
{
    return Results.Content("hello world");
}).WithMetadata(new FeatureGateAttribute("myfeature"));
```


#### 在QuickApi中使用FeatureManagement

```csharp
[FeatureGate("myfeature")]
[QuickApi("feature-test2")]
public class FeatureApi : BaseQuickApi<EmptyRequest, string>
{
	public override async ValueTask<string> ExecuteAsync(EmptyRequest request)
	{
		await Task.CompletedTask;
		return "hello world";
	}
}
```

#### More


[更多用例](https://github.com/microsoft/FeatureManagement-Dotnet/tree/main/examples)
