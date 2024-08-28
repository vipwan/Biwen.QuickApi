### 多租户模块使用方式

#### 1. 实现租户信息提供者接口
```csharp
internal class MyTenantInfoProvider : ITenantInfoProvider<TenantInfo>
{
    public Task<IList<TenantInfo>> GetAllAsync()
    {
        return Task.FromResult<IList<TenantInfo>>(new List<TenantInfo>
        {
            new TenantInfo
            {
                Id = "tenant1",
                Name = "租户1",
                Identifier = "tenant1",
                ConnectionString = "Data Source=.;Initial Catalog=Db1;Integrated Security=True"
            },
            new TenantInfo
            {
                Id = "tenant2",
                Name = "租户2",
                Identifier = "tenant2",
                ConnectionString = "Data Source=.;Initial Catalog=Db2;Integrated Security=True"
            }
        });
    }
}
```
#### 2. 注册多租户模块
```csharp

//注册多租户模块,必须添加租户信息提供者和租户查找器
//当前用例中使用基于路径的租户查找器,你可以根据具体业务使用其他查找器(路由,Session,Host,Header),或者自定义查找器
services.AddMultiTenant<TenantInfo>().AddTenantInfoProvider<MyTenantInfoProvider>().AddBasePathTenantFinder();

```

#### 3. 使用多租户模块

任何使用多租户的业务请务必在`UseMultiTenant<T>`后注册,否则无法获取租户信息

```csharp

//如果需要多租户支持,UseMultiTenant必须在UseBiwenQuickApis,等中间件之前完成
app.UseMultiTenant<TenantInfo>();

```
然后使用HttpContext扩展方法获取租户信息

```csharp
var tenantInfo = HttpContext!.GetTenantInfo<TenantInfo>();
```

如果你不使用中间件的方式读取多租户信息,也可以注入`ITenantFinder<T>`按需获取租户信息
```csharp

public class MyService(ITenantFinder<TenantInfo> tenantFinder)
{
	public async Task DoSomething()
	{
		var tenant = await tenantFinder.FindAsync();
		//do something
	}
}

```