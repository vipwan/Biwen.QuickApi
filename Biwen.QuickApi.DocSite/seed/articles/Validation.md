数据验证
=====================

目前请求的Request支持数据验证,同时支持`FluentValidation`和MS`DataAnnotations`的验证方式,在Request对象中继承BaseRequest,并且在构造函数中调用RuleFor方法进行验证规则的设置,如下所示:

```csharp
public class HelloApiRequest : BaseRequest<HelloApiRequest>
{
	[Description("Name Desc")]
	public string Name { get; set; } = null!;

	/// <summary>
	/// FromQuery特性绑定字段
	/// </summary>
	[FromQuery("q"),Required]
	public string? Q { get; set; }

	public HelloApiRequest()
	{
		RuleFor(x => x.Name).NotNull().Length(5, 10);
	}
}
```
使用DataAnnotations的方式,可以直接在属性上使用`Required`等特性,如上所示,`Q`属性使用了`Required`特性,表示这个属性是必填的,如果不填写,则会返回400错误。
使用FluentValidation的方式,在构造函数中调用`RuleFor`方法,设置验证规则,如上所示,`Name`属性设置了`NotNull`和`Length`验证规则,表示这个属性不能为空,并且长度在5-10之间。

> [!NOTE]
> Request的验证不仅可以在QuickApi和QuickEndpoint中使用,你可以在任意地方调用`Validate`方法进行验证,如下所示:

```csharp
var request = new HelloApiRequest
{
	Name = "Tom",
	Q = "Hello"
};
var result = request.Validate();
if (!result.IsValid)
{
	//验证失败
}
else
{
	//验证成功
}
```

API文档
---------------------

相关API文档:
[BaseRequest](../api/Biwen.QuickApi.BaseRequest-1.yml)