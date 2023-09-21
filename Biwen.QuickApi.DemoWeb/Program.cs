using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBiwenQuickApis();
builder.Services.AddAuthentication("Bearer");
builder.Services.AddAuthorization(builder => builder.AddPolicy("admin", policy => policy.RequireClaim("admin")));

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
builder.Services.AddScoped<HelloService>();


var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.UseWelcomePage("/");


//swagger
app.UseSwagger();
app.UseSwaggerUI();


app.MapBiwenQuickApis();


//测试其他地方调用QuickApi
app.MapGet("/fromapi", (Biwen.QuickApi.DemoWeb.Apis.Hello4Api api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var validator = req.RealValidator as IValidator<EmptyRequest>;
    if (validator != null)
    {
        var result = validator.Validate(req);
        if (!result.IsValid)
        {
            return Results.BadRequest(result.ToDictionary());
        }
    }
    //执行请求
    var x = api.Execute(new EmptyRequest());
    return Results.Ok(x);
});


app.Run();
