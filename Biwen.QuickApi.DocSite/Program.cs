using Docfx.Dotnet;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateSlimBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = "Cookies";
    o.DefaultChallengeScheme = "Cookies";
}).AddCookie();
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddBiwenQuickApis(o =>
{
    o.UseQuickApiExceptionResultBuilder = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

//use quick api
app.UseBiwenQuickApis();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/docs",
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Documents")),
    ServeUnknownFileTypes = true
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "_statics")),
    ServeUnknownFileTypes = true
});

var options = new DotnetApiOptions
{
    IncludeApi = symbol =>
    {
        return symbol.ToDisplayString() switch
        {
            null => SymbolIncludeState.Exclude,
            { } ns when ns.StartsWith("Biwen.QuickApi.DocSite") => SymbolIncludeState.Exclude,
            _ => SymbolIncludeState.Include,
        };
    },
};

//当变更的时候生成yaml文件:
//await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("seed/docfx.json", options);

//静态生成:
await Docfx.Docset.Build("seed/docfx.json");

app.MapGet("/", () => { return Results.Redirect("/index.html"); }).ExcludeFromDescription();

app.Run();
