using BenchmarkTestWeb;
using Biwen.QuickApi;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "";
});


var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    //swagger ui
    //

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.UseWelcomePage("/");
}

app.Use(async (context, next) =>
{
    //统一等待10微秒
    //await Task.Delay(TimeSpan.FromMicroseconds(10));
    //记录日志
    Console.WriteLine($"{context.Request.Path.Value} {context.Request.Host.Value}:{context.Request.ContentType}");
    await next(context);
});

//minimal
app.MapPost("/my-minimal", ([Microsoft.AspNetCore.Mvc.FromBody] MyRequest request) =>
{
    var validResult = request.Validate();
    if (!validResult.IsValid)
    {
        return Results.BadRequest(validResult.ToDictionary());
    }
    return Results.Ok(request);
}).WithTags("API");


//default方式
app.MapBiwenQuickApis();

app.UseAuthorization();
app.MapControllers();

app.Run();

namespace BenchmarkTestWeb
{
    public partial class Program { }
}