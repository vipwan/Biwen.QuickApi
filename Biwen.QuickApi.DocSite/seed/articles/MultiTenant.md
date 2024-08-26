MultiTenant
=====================

多租户模块

实现租户信息提供者接口
---------------------
实现`ITenantInfoProvider<T>`请务必考虑缓存等性能问题,多租户信息不会频繁变动,可以适当缓存

```csharp
internal class MyTenantInfoProvider : ITenantInfoProvider<TenantInfo>
{
    public Task<IList<TenantInfo>> GetAll()
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

注册多租户模块
---------------------

注册多租户模块,必须添加租户信息提供者和租户查找器
当前用例中使用基于路径的租户查找器,你可以根据具体业务使用其他查找器(路由,Session,Host,Header),或者自定义查找器

```csharp

services.AddMultiTenant<TenantInfo>().AddTenantInfoProvider<MyTenantInfoProvider>().AddBasePathTenantFinder();

```
当查找不到租户信息时,可以使用配置项默认租户Id,请传递`MultiTenantOptions`选项!


使用多租户模块
---------------------

任何使用多租户的业务请务必在`UseMultiTenant<T>`后注册,否则无法获取租户信息

```csharp

//如果需要多租户支持,UseMultiTenant必须在UseBiwenQuickApis,等中间件之前完成
app.UseMultiTenant<TenantInfo>();

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



API文档
---------------------
相关API文档:
[ITenantInfo](../api/Biwen.QuickApi.MultiTenant.ITenantInfo.yml) &nbsp;
[ITenantInfoProvider](../api/Biwen.QuickApi.MultiTenant.Abstractions.ITenantInfoProvider-1.yml)&nbsp;
[ITenantFinder](../api/Biwen.QuickApi.MultiTenant.Abstractions.ITenantFinder-1.yml)&nbsp;