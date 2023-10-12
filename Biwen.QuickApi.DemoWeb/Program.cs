using Biwen.QuickApi.DemoWeb;
using Biwen.QuickApi.DemoWeb.Apis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddOutputCache();

//builder.Services.AddAuthorizationBuilder().AddPolicy("admin", policy =>
//{
//    policy.RequireClaim("admin");
//    policy.RequireAuthenticatedUser();
//});

builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("admin", policy =>
    {
        policy.RequireClaim("admin");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.Configure<AuthenticationOptions>(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
});

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/login";
//});

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
builder.Services.AddScoped<HelloService>();
// keyed services
builder.Services.AddKeyedScoped<HelloService>("hello");

//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    //不需要驼峰模式设置为null
    //o.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();


//swagger
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

// 默认方式
var apis = app.MapBiwenQuickApis();
//如果你想对特定的分组批量操作. 比如授权等,可以这样做,但是注意该操作会覆盖掉原有的配置(如果存在的情况下)
var groupAdmin = apis.FirstOrDefault(x => x.Group == "admin");
groupAdmin.RouteGroupBuilder?
    .WithTags("Admin Test")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "用于测试权限相关",
        //Description = "Admin Test"
    })                             //自定义OpenApi
    .RequireHost("localhost:5101") //模拟需要指定Host访问接口
    ;

// Gen方式
//app.MapGenQuickApis(app.Services);

//app.UseWelcomePage("/");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


//测试其他地方调用QuickApi
app.MapGet("/fromapi",
    async Task<Results<Ok<string>, BadRequest<IDictionary<string, string[]>>>>
    (JustAsService api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var result = req.RealValidator.Validate(req);
    if (!result.IsValid)
    {
        return TypedResults.BadRequest(result.ToDictionary());
    }

    //执行请求
    var x = await api.ExecuteAsync(req);
    return TypedResults.Ok(x.Content);

}).RequireAuthorization("admin");

//app.MapGet("hhe", () => TypedResults.Ok(new EmptyResponse()));


//发现ms的WithOpenApi的一处BUG,当Method为多个时会报错!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});



app.Run();
