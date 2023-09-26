using Biwen.QuickApi;
using Biwen.QuickApi.SourceGenerator.TestConsole;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBiwenQuickApis(builder =>
{
    builder.RoutePrefix = "";
    //builder.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();



app.MapGet("/", () => Results.Content("hello world")).ExcludeFromDescription();

//app.MapBiwenQuickApis();


app.MapQuickApis();

app.Run();
