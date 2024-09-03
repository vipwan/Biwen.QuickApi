using Biwen.QuickApi.DemoWeb.GroupRouteBuilders;
using Biwen.QuickApi.OpenApi;
using Biwen.QuickApi.OpenApi.Scalar;
using Biwen.QuickApi.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Reflection;
using Biwen.QuickApi.MultiTenant;

//verison 
Console.WriteLine($"Biwen.QuickApi Version:{Biwen.QuickApi.Generated.Version.Current}");

var builder = WebApplication.CreateBuilder(args);


#region Logger

builder.Logging.ClearProviders();


//使用配置文件Serilog注册
builder.Host.UseSerilogFromConfiguration();



#endregion

builder.Services.AddFluentUIComponents();

//add razor pages
builder.Services.AddRazorPages();

//add mvc
builder.Services.AddControllersWithViews();


//all
builder.Services.AddOpenApi("v1", onlyQuickApi: false);
//just quickapi & group:[test,admin]
builder.Services.AddOpenApi("v2", onlyQuickApi: true, ["test", "admin"]);

builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("admin", configurePolicy: policy =>
    {
        //policy.RequireClaim("admin", "admin");
        policy.RequireAuthenticatedUser();
    });
});


//在DbModular中注册

//builder.Services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase("net8"));
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


//可通过配置文件配置
builder.Services.AddBiwenQuickApis(o =>
{
    o.UseQuickApiExceptionResultBuilder = true;
});

//如果需要自定义分组路由构建器
builder.Services.AddQuickApiGroupRouteBuilder<DefaultGroupRouteBuilder>();
//如果需要自定义异常返回格式
//builder.Services.AddSingleton<IQuickApiExceptionResultBuilder, CustomExceptionResultBuilder>();
//自定义异常处理
builder.Services.AddScoped<IQuickApiExceptionHandler, CustomExceptionHandler>();

var app = builder.Build();

//如果需要多租户支持,UseMultiTenant必须在UseBiwenQuickApis,等中间件之前完成
app.UseMultiTenant<TenantInfo>();

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

app.UseBiwenQuickApis();

//map razor pages
app.MapRazorPages();

//mvc
app.MapControllerRoute("default_route", "{area}/{controller}/{action}/{id?}",
    defaults: new { area = "MyArea", controller = "Home", action = "Index" },
    constraints: new { area = "MyArea" });
app.MapControllerRoute("default_route", "{controller}/{action}/{id?}");


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