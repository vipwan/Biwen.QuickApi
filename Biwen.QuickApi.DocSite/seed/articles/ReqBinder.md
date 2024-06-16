请求绑定
=====================

请求绑定是指将请求参数绑定到请求对象的过程,QuickApi默认的方式是使用特性的方式进行绑定,如下所示:

```csharp
public class HelloApiRequest : BaseRequest<HelloApiRequest>
{
	[Description("Name Desc")]
	public string? Name { get; set; }

	/// <summary>
	/// FromQuery特性绑定字段
	/// </summary>
	[FromQuery("q")]
	public string? Q { get; set; }
}
```
在上面的代码中,`Q`属性使用了`FromQuery`特性,表示这个属性是从Query中绑定的,
如果没有设置FromXXX,则绑定逻辑会依次从`Route`,`Query`,`Body`中进行绑定。
如果请求的对象完全来自POST的body部分,则可以使用`FromBody`特性,如下所示:

```csharp
[FromBody]
public class FromBodyRequest : BaseRequest<FromBodyRequest>
{
	public int Id { get; set; }
	public string? Name { get; set; }
}
```
POST的内容也必须是json格式,如下所示:

```json
{
	"id": 1,
	"name": "Tom"
}
```


自定义绑定
---------------------

如果默认的绑定方式不能满足你的需求,你可以实现`IReqBinder<T>`接口,如下所示:

```csharp
public class CustomApiRequest : BaseRequest<CustomApiRequest>, IReqBinder<CustomApiRequest>
{
	public string? Name { get; set; }
	public CustomApiRequest()
	{
		RuleFor(x => x.Name).NotNull().Length(5, 10);
	}
	public static ValueTask<T> BindAsync(HttpContext context, ParameterInfo parameter = null!)
	{
		var request = new CustomApiRequest();
		//从HttpContext中绑定请求对象:
		request.Name = context.Request.Query["name"];
		return Task.FromResult(request);
	}
}
```

然后在QuickApi中使用,如下所示:

```csharp
public class CustomApi : QuickApi<CustomApiRequest>
{
	public CustomApi()
    {
        UseReqBinder<CustomApiRequest>();
	}
	public override async ValueTask<IResult> ExecuteAsync(CustomApiRequest request)
    {
        await Task.CompletedTask;
        return Results.Content($"{request.Name}");
    }
}
```

> [!NOTE]
> 请注意`BindAsync`是接口静态成员,意味着你可以在任何地方使用,如一下代码:

```csharp
var context = Request.HttpContext!;
//从已知的HttpContext中绑定请求对象:
var request = await CustomApiRequest.BindAsync(context);
//验证请求对象:
var result = request.Validate();

//其他逻辑:

```

API文档
---------------------

相关API文档:

[IReqBinder](../api/Biwen.QuickApi.Abstractions.IReqBinder-1.yml) &nbsp;
[DefaultReqBinder](../api/Biwen.QuickApi.DefaultReqBinder-1.yml) &nbsp;