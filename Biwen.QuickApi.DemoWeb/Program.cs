using Biwen.QuickApi.DemoWeb.Apis.Endpoints;
using Biwen.QuickApi.DemoWeb.GroupRouteBuilders;
using Biwen.QuickApi.DemoWeb.Schedules;
using Biwen.QuickApi.OpenApi;
using Biwen.QuickApi.OpenApi.Scalar;
using Biwen.QuickApi.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.UseTransformer<BearerSecuritySchemeTransformer>();
    options.ShouldInclude = (desc) => true;
});

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
         options.LoggingFields = HttpLoggingFields.Request;
         options.CombineLogs = true;
     });
});

// Add services to the container.
builder.Services.AddScoped<HelloService>();
// keyed services
//builder.Services.AddKeyedScoped<HelloService>("hello");

// Add ScheduleTaskStore
builder.Services.AddScheduleMetadataStore<DemoStore>();

//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    o.EnableAntiForgeryTokens = true;
    o.EnablePubSub = true;
    o.EnableScheduling = true;
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

    builder.MapGet("/", () => Results.Redirect("openapi/scalar/v1")).ExcludeFromDescription();

}, builder =>
{
    builder.UseWelcomePage("/");
});

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.UseResponseCaching();

// 默认方式
//app.MapBiwenQuickApis();
app.UseBiwenQuickApis();

app.MapGet("/binder", (HttpContext context, BindRequest request) =>
{
    //测试默认绑定器
    return Results.Content(request.Hello);
});

//提供IQuickEndpoint支持:
app.MapGroup("endpoints", x =>
{
    //~/endpoints/hello/hello?key=world
    x.MapMethods<HelloEndpoint>("hello/{hello}");
    x.MapMethods<PostDataEndpoint>("hello/postdata");
});


// Identity API {"email" : "vipwan@co.ltd","password" : "*******"}
// ~/account/register    
// ~/account/login 

app.UseIfElse(app.Environment.IsDevelopment(), builder =>
{
    builder.MapGroup("account").MapIdentityApi<IdentityUser>().ExcludeFromDescription();//swagger
}, builder =>
{
    builder.MapGroup("account").MapIdentityApi<IdentityUser>().ExcludeFromDescription();
});

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