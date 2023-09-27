﻿using Biwen.QuickApi;
using Biwen.QuickApi.SourceGenerator.TestConsole;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBiwenQuickApis(builder =>
{
    builder.RoutePrefix = "";
    //builder.JsonSerializerOptions.PropertyNamingPolicy = null;
});

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();



app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

//app.MapBiwenQuickApis();

//swagger
app.UseSwagger();
app.UseSwaggerUI();


app.MapGroup("").MapGet("/world", (IHttpContextAccessor ctx) => Results.Content("hello world")).ExcludeFromDescription();
app.MapGroup("hello").MapGet("/world2", () => Results.Content("hello world2")).ExcludeFromDescription();


app.MapGenQuickApis(app.Services);

//
//app.MapGenQuickApis();

app.Run();
