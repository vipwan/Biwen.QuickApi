using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer");
builder.Services.AddAuthorization(builder => builder.AddPolicy("admin", policy => policy.RequireClaim("admin")));

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
builder.Services.AddScoped<HelloService>();

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

//app.UseWelcomePage("/");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


// var apis = app.MapBiwenQuickApis();
//
app.MapGenQuickApis(app.Services);
//如果你想对特定的分组批量操作. 比如授权等,可以这样做

//测试其他地方调用QuickApi
app.MapGet("/fromapi", async (JustAsService api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var result = req.RealValidator.Validate(req);
    if (!result.IsValid)
    {
        return Results.BadRequest(result.ToDictionary());
    }


    //执行请求
    var x = await api.ExecuteAsync(new EmptyRequest());
    return Results.Content(x.ToString());
});


//发现ms的WithOpenApi的一处BUG,当Method为多个时会报错!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});



app.Run();
