using System.Text.Json.Serialization;
using Biwen.QuickApi;

var builder = WebApplication.CreateSlimBuilder(args);



builder.Services.AddBiwenQuickApis(x =>
{
    x.RoutePrefix = "";
});

//如果需要单独设置Json序列化配置项,请使用如下方式
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapBiwenQuickApis();

app.Run();


#pragma warning disable CA1050 // 在命名空间中声明类型
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
#pragma warning restore CA1050 // 在命名空间中声明类型

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
