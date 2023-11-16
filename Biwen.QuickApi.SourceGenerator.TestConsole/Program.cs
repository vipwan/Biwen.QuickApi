using Biwen.QuickApi;
using Biwen.QuickApi.SourceGenerator.TestConsole.GroupRouteBuilders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBiwenQuickApis(builder =>
{
    builder.RoutePrefix = "";
    //builder.JsonSerializerOptions.PropertyNamingPolicy = null;
});

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddQuickApiDocument(cfg =>
{
    cfg.DocumentName = "QuickApi Test";
});

builder.Services.AddBiwenQuickApiGroupRouteBuilder<DefaultGroupRouteBuilder>();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


//swagger
app.UseQuickApiSwagger(uiConfig: cfg =>
{
    cfg.DefaultModelsExpandDepth = -1;
});


app.MapGroup("").MapGet("/world", (IHttpContextAccessor ctx) => Results.Content("hello world")).ExcludeFromDescription();
app.MapGroup("hello").MapGet("/world2", () => Results.Content("hello world2")).ExcludeFromDescription();


//app.MapBiwenQuickApis();
app.MapGenQuickApis(app.Services);


app.Run();
