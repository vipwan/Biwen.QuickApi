using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;
using Microsoft.OpenApi.Models;
using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer");
builder.Services.AddAuthorization(builder => builder.AddPolicy("admin", policy => policy.RequireClaim("admin")));

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0", new OpenApiInfo
    {
        Version = "1.0",
        Title = "API1标题",
        Description = $"API描述,{"1.0"}版本, ?api-version=1.0"
    });

    options.SwaggerDoc("2.0", new OpenApiInfo
    {
        Version = "2.0",
        Title = "API2标题",
        Description = $"API描述,{"2.0"}版本, ?api-version=2.0"
    });
});


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

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/1.0/swagger.json", "1.0");
    options.SwaggerEndpoint($"/swagger/2.0/swagger.json", "2.0");
});


app.UseAuthentication();
app.UseAuthorization();

//app.UseWelcomePage("/");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


app.MapBiwenQuickApis();


//测试其他地方调用QuickApi
app.MapGet("/fromapi", async (Biwen.QuickApi.DemoWeb.Apis.JustAsService api) =>
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


app.Run();
