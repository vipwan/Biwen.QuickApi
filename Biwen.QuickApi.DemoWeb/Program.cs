﻿using Biwen.QuickApi.DemoWeb.GroupRouteBuilders;
using Biwen.QuickApi.OpenApi;
using Biwen.QuickApi.OpenApi.Scalar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


#region Logger

builder.Logging.ClearProviders();

builder.Host.UseSerilog((context, builder) =>
{
    builder.ReadFrom
      .Configuration(context.Configuration)
      .Enrich
      .FromLogContext();
});

#endregion



//FluentUIComponents
builder.Services.AddFluentUIComponents();
builder.Services.AddHttpClient();//default httpclient

//all
builder.Services.AddOpenApi("v1", onlyQuickApi: false);
//just quickapi & group:[test,admin]
builder.Services.AddOpenApi("v2", onlyQuickApi: true, ["test", "admin"]);


builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = "Cookies";
    o.DefaultChallengeScheme = "Cookies";
}).AddCookie();

builder.Services.AddAuthorization();


builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("admin", configurePolicy: policy =>
    {
        //policy.RequireClaim("admin", "admin");
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

builder.Services.AddIf(builder.Environment.IsDevelopment(), sp =>
{
    sp.AddHttpLogging(options =>
     {
         options.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.RequestQuery;
         options.CombineLogs = true;
     });
});


//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";//默认QuickApi的路由前缀
    o.EnableAntiForgeryTokens = true;//默认启动防伪
    o.EnablePubSub = true;//默认启动发布订阅
    o.EnableScheduling = true;//默认启动定时任务
    o.UseQuickApiExceptionResultBuilder = true;
});

//如果需要自定义分组路由构建器
builder.Services.AddQuickApiGroupRouteBuilder<DefaultGroupRouteBuilder>();
//如果需要自定义异常返回格式
//builder.Services.AddSingleton<IQuickApiExceptionResultBuilder, CustomExceptionResultBuilder>();
//自定义异常处理
builder.Services.AddScoped<IQuickApiExceptionHandler, CustomExceptionHandler>();

var app = builder.Build();

app.UseIfElse(app.Environment.IsDevelopment(), builder =>
{
    //Http Logging
    builder.UseHttpLogging();
    builder.UseDeveloperExceptionPage();

    app.MapGroup("openapi", app =>
    {
        //swagger ui
        app.MapOpenApi("{documentName}.json");
        app.MapScalarUi();
    });

    builder.MapGet("/", () => Results.Redirect("root/razor/welcome")).ExcludeFromDescription();

}, builder =>
{
    builder.UseWelcomePage("/");
});

app.UseAuthentication();
app.UseAuthorization();

// 默认方式
//app.MapBiwenQuickApis();
app.UseBiwenQuickApis();

app.Run();

//用于xunit Test
namespace Biwen.QuickApi.DemoWeb
{
    public partial class Program { }


    public partial class BindRequest : BaseRequest<BindRequest>, IReqBinder<BindRequest>
    {
        public string? Hello { get; set; }

        public static async ValueTask<BindRequest> BindAsync(HttpContext context, ParameterInfo parameter = null!)
        {
            //返回默认绑定
            var req = await DefaultReqBinder<BindRequest>.BindAsync(context, parameter);
            req!.Hello = "test binder";
            return req;
        }
    }
}