using Biwen.QuickApi.DemoWeb.Apis;
using Biwen.QuickApi.DemoWeb.GroupRouteBuilders;
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = "Cookies";
    o.DefaultChallengeScheme = "Cookies";
}).AddCookie();

builder.Services.AddAuthorization();

builder.Services.AddOutputCache(options =>
{
    //options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
});

builder.Services.AddResponseCaching();

builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("admin", configurePolicy: policy =>
    {
        policy.RequireClaim("admin", "admin");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase("net8"));
// adds a set of common identity services to the application
builder.Services.AddIdentityApiEndpoints<IdentityUser>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Password.RequiredUniqueChars = 0;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireDigit = false;
    o.Password.RequireUppercase = false;

}).AddEntityFrameworkStores<IdentityDbContext>();


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


builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.CombineLogs = true;
});


// Add services to the container.
builder.Services.AddScoped<HelloService>();
// keyed services
//builder.Services.AddKeyedScoped<HelloService>("hello");

//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    o.EnableAntiForgeryTokens = true;
});

//如果需要自定义分组路由构建器
builder.Services.AddBiwenQuickApiGroupRouteBuilder<DefaultGroupRouteBuilder>();
//如果需要自定义异常返回格式
//builder.Services.AddSingleton<IQuickApiExceptionResultBuilder, CustomExceptionResultBuilder>();
//自定义异常处理
builder.Services.AddScoped<IQuickApiExceptionHandler, CustomExceptionHandler>();

var app = builder.Build();


if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();

    //swagger ui
    app.UseQuickApiSwagger(uiConfig: cfg =>
    {
        //cfg.CustomJavaScriptPath = "/miniprofiler-sc";
    });

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


if (!app.Environment.IsProduction())
{
    //Http Logging
    app.UseHttpLogging();
}

// 默认方式
//app.MapBiwenQuickApis();
app.UseBiwenQuickApis();

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

// Identity API {"email" : "vipwan@co.ltd","password" : "*******"}
// ~/account/register    
// ~/account/login 

if (app.Environment.IsDevelopment())
{
    app.MapGroup("account").MapIdentityApi<IdentityUser>().WithOpenApi();//swagger
}
else
{
    app.MapGroup("account").MapIdentityApi<IdentityUser>();
}


//发现ms的WithOpenApi的一处BUG,当Method为多个时会报错!
//请直接使用QuickApiSummaryAttribute!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});


app.Run();


//用于xunit Test
namespace Biwen.QuickApi.DemoWeb
{
    public partial class Program { }
}