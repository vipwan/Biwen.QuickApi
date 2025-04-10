using Biwen.QuickApi.Contents;
using Biwen.QuickApi.DocSite;
using Docfx.Dotnet;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateSlimBuilder(args);

// Add services to the container.
builder.Services.AddBiwenQuickApis(o =>
{
    o.UseQuickApiExceptionResultBuilder = true;
});


builder.Services.AddDbContext<TestDbContext>(o =>
{
});
builder.Services.AddBiwenContents<TestDbContext>();

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

// redirect to index.html
app.MapGet("/", () => Results.Redirect("index.html")).ExcludeFromDescription();


// Options
(bool GenApiYml, bool GenDoc) genOptions = (false, true);

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

if (genOptions.GenApiYml)
{
    //生成api.yml文件
    await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("seed/docfx.json", options);
}
if (genOptions.GenDoc)
{
    //静态生成:
    await Docfx.Docset.Build("seed/docfx.json");
}
app.Run();
