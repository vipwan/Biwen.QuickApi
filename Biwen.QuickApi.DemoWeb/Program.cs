using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBiwenQuickApis();
builder.Services.AddAuthentication("Bearer");
builder.Services.AddAuthorization(builder => builder.AddPolicy("admin", policy => policy.RequireClaim("admin")));


// Add services to the container.
builder.Services.AddScoped<HelloService>();


var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.UseWelcomePage("/");


app.MapBiwenQuickApis();

app.Run();
