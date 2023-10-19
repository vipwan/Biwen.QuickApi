using Biwen.QuickApi.DemoWeb;
using Biwen.QuickApi.DemoWeb.Apis;
using Biwen.QuickApi.SourceGenerator;
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddOutputCache(options =>
{
    //options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
});

builder.Services.AddResponseCaching();

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


#region swagger 文档


//swagger
builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.DocumentName = "Quick API ALL QuickApi";

    //options.ApiGroupNames = new[] { };//未指定展示全部Api

    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API ALL QuickApi",
            Title = "Quick API testcase",
            Description = "Biwen.QuickApi 测试用例",
            TermsOfService = "https://github.com/vipwan",
            Contact = new OpenApiContact
            {
                Name = "欢迎 Star & issue",
                Url = "https://github.com/vipwan/Biwen.QuickApi"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = "https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt"
            }
        };
    };
},
new SecurityOptions(), true);

builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.DocumentName = "Quick API Admin&Group1";

    options.ApiGroupNames = new[] { "admin", "group1" }; //文档分组指定

    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API V2",
            Title = "Quick API testcase",
            Description = "Biwen.QuickApi 测试用例",
            TermsOfService = "https://github.com/vipwan",
            Contact = new OpenApiContact
            {
                Name = "欢迎 Star & issue",
                Url = "https://github.com/vipwan/Biwen.QuickApi"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = "https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt"
            }
        };
    };
},
new SecurityOptions());


builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.DocumentName = "Quick API ALL";

    //options.ApiGroupNames = new[] { };//未指定展示全部Api

    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API ALL",
            Title = "Quick API testcase",
            Description = "Biwen.QuickApi 测试用例",
            TermsOfService = "https://github.com/vipwan",
            Contact = new OpenApiContact
            {
                Name = "欢迎 Star & issue",
                Url = "https://github.com/vipwan/Biwen.QuickApi"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = "https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt"
            }
        };
    };
},
new SecurityOptions());


#endregion




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

//如果需要自定义异常返回格式
builder.Services.AddSingleton<IQuickApiExceptionResultBuilder, CustomExceptionResultBuilder>();

//自定义异常处理
builder.Services.AddScoped<IQuickApiExceptionHandler, CustomExceptionHandler>();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    //swagger ui
    app.UseQuickApiSwagger();

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.UseWelcomePage("/");
}

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.UseResponseCaching();

// 默认方式
var apis = app.MapBiwenQuickApis();
//如果你想对特定的分组批量操作. 比如授权等,可以这样做,但是注意该操作会覆盖掉原有的配置(如果存在的情况下)
var groupAdmin = apis.FirstOrDefault(x => x.Group == "admin");
groupAdmin.RouteGroupBuilder?
    .WithTags("authorization")         //自定义Tags
                                       //.RequireHost("localhost:5101")   //模拟需要指定Host访问接口
    .WithGroupName("admin")            //自定义EndpointGroupName
    ;

// Gen方式
//app.MapGenQuickApis(app.Services);

//测试其他地方调用QuickApi
app.MapGet("/fromapi", async Task<Results<Ok<string>, BadRequest<IDictionary<string, string[]>>>>
    (JustAsService api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var result = req.Validate();
    if (!result.IsValid)
    {
        return TypedResults.BadRequest(result.ToDictionary());
    }

    //执行请求
    var x = await api.ExecuteAsync(req);
    return TypedResults.Ok(x.Content);

});

//发现ms的WithOpenApi的一处BUG,当Method为多个时会报错!
//请直接使用QuickApiSummaryAttribute!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});


app.Run();
