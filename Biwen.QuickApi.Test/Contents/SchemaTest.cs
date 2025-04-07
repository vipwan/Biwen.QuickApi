// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: 万雅虎, Github: https://github.com/vipwan
// 
// Modify Date: 2025-04-04 16:16:23 SchemaTest.cs

using Biwen.QuickApi.Contents;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.FieldTypes;
using Biwen.QuickApi.Contents.Schema;
using Biwen.QuickApi.DemoWeb.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Biwen.QuickApi.Test.Contents;

public class SchemaTest
{
    private readonly ITestOutputHelper _output;

    public SchemaTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private IMemoryCache GetMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }


    #region FormilySchemaGenerator


    [Fact]
    public void ContentSchemaGenerator_Should_Register_Successfully()
    {
        // 注册
        var services = new ServiceCollection();

        //注册内存数据库
        services.AddDbContext<MyDbcontext>(options => options.UseInMemoryDatabase("TestDb"));

        services.AddBiwenContents<MyDbcontext>();
        var serviceProvider = services.BuildServiceProvider();

        // 执行
        var schemaGenerator = serviceProvider.GetService<IContentSchemaGenerator>();

        // 断言
        Assert.NotNull(schemaGenerator);
        Assert.IsType<XRenderSchemaGenerator>(schemaGenerator);
    }

    [Fact]
    public void GenerateSchema_Should_Return_Valid_JsonObject_For_BlogType()
    {
        // 安排
        var schemaGenerator = new FormilySchemaGenerator(GetMemoryCache());

        // 执行
        var schema = schemaGenerator.GenerateSchema<Blog>();

        // 断言
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
        Assert.NotNull(schema["properties"]);

        var properties = schema["properties"] as JsonObject;
        Assert.NotNull(properties);

        // 验证标题属性
        Assert.NotNull(properties["Title"]);
        Assert.Equal("string", properties["Title"]?["type"]?.ToString());
        Assert.Equal("Input", properties["Title"]?["x-component"]?.ToString());
        Assert.Equal("500", properties["Title"]?["x-component-props"]?["maxLength"]?.ToString());
        Assert.True(properties["Title"]?["required"]?.GetValue<bool>());

        // 验证发布状态属性
        Assert.NotNull(properties["IsPublished"]);
        Assert.Equal("boolean", properties["IsPublished"]?["type"]?.ToString());
        Assert.Equal("Switch", properties["IsPublished"]?["x-component"]?.ToString());
        Assert.True(properties["IsPublished"]?["required"]?.GetValue<bool>());

        // 验证描述属性
        Assert.NotNull(properties["Description"]);
        Assert.Equal("string", properties["Description"]?["type"]?.ToString());
        Assert.Equal("TextArea", properties["Description"]?["x-component"]?.ToString());
        Assert.Equal("500", properties["Description"]?["x-component-props"]?["maxLength"]?.ToString());

        // 验证内容属性
        Assert.NotNull(properties["Content"]);
        Assert.Equal("string", properties["Content"]?["type"]?.ToString());
        Assert.Equal("Markdown", properties["Content"]?["x-component"]?.ToString());

        // 验证标签属性
        Assert.NotNull(properties["Tags"]);
        Assert.Equal("array", properties["Tags"]?["type"]?.ToString());
        Assert.Equal("ArrayItems", properties["Tags"]?["x-component"]?.ToString());
        Assert.True(properties["Tags"]?["required"]?.GetValue<bool>());
        Assert.NotNull(properties["Tags"]?["items"]);
        Assert.Equal("string", properties["Tags"]?["items"]?["type"]?.ToString());
        Assert.Equal("Input", properties["Tags"]?["items"]?["x-component"]?.ToString());
    }

    [Fact]
    public void GenerateSchemaJson_Should_Return_Valid_Json_String()
    {
        // 安排
        var schemaGenerator = new FormilySchemaGenerator(GetMemoryCache());

        // 执行
        var schemaJson = schemaGenerator.GenerateSchemaJson<Blog>();

        // 输出生成的JSON以便检查
        _output.WriteLine(schemaJson);

        // 断言
        Assert.NotNull(schemaJson);
        Assert.True(schemaJson.Length > 0);

        // 尝试反序列化以验证JSON有效性
        var exception = Record.Exception(() => JsonDocument.Parse(schemaJson));
        Assert.Null(exception);
    }

    [Fact]
    public void GenerateSchema_Should_Throw_Exception_For_NonIContentType()
    {
        // 安排
        var schemaGenerator = new FormilySchemaGenerator(GetMemoryCache());

        // 执行和断言
        var exception = Assert.Throws<ArgumentException>(() => schemaGenerator.GenerateSchema(typeof(NonIContentClass)));
        Assert.Contains("必须实现 IContent 接口", exception.Message);
    }

    [Fact]
    public void GenerateSchema_Should_Handle_CustomContent()
    {
        // 安排
        var schemaGenerator = new FormilySchemaGenerator(GetMemoryCache());

        // 执行
        var schema = schemaGenerator.GenerateSchema<CustomContent>();
        var schemaJson = schemaGenerator.GenerateSchemaJson<CustomContent>();

        _output.WriteLine(schemaJson);

        // 断言
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());

        var properties = schema["properties"] as JsonObject;
        Assert.NotNull(properties);

        // 验证基本属性
        Assert.NotNull(properties["Name"]);
        Assert.Equal("string", properties["Name"]?["type"]?.ToString());
        Assert.Equal("Input", properties["Name"]?["x-component"]?.ToString());
        Assert.True(properties["Name"]?["required"]?.GetValue<bool>());
        Assert.Equal("100", properties["Name"]?["x-component-props"]?["maxLength"]?.ToString());

        // 验证年龄属性及其范围验证
        Assert.NotNull(properties["Age"]);
        Assert.Equal("number", properties["Age"]?["type"]?.ToString());
        Assert.Equal("NumberPicker", properties["Age"]?["x-component"]?.ToString());
        Assert.Equal(1.0, properties["Age"]?["minimum"]?.GetValue<double>());
        Assert.Equal(120.0, properties["Age"]?["maximum"]?.GetValue<double>());

        // 验证简介属性
        Assert.NotNull(properties["Bio"]);
        Assert.Equal("string", properties["Bio"]?["type"]?.ToString());
        Assert.Equal("TextArea", properties["Bio"]?["x-component"]?.ToString());
        Assert.Equal("1000", properties["Bio"]?["x-component-props"]?["maxLength"]?.ToString());

        // 验证电话属性及其验证器
        Assert.NotNull(properties["Phone"]);
        Assert.Equal("string", properties["Phone"]?["type"]?.ToString());
        Assert.Equal("Input", properties["Phone"]?["x-component"]?.ToString());
        Assert.Equal("phone", properties["Phone"]?["x-validator"]?.ToString());
        Assert.Equal("tel", properties["Phone"]?["format"]?.ToString());

        // 验证邮箱属性及其验证器
        Assert.NotNull(properties["Email"]);
        Assert.Equal("string", properties["Email"]?["type"]?.ToString());
        Assert.Equal("Input", properties["Email"]?["x-component"]?.ToString());
        Assert.Equal("email", properties["Email"]?["x-validator"]?.ToString());
        Assert.Equal("email", properties["Email"]?["format"]?.ToString());

        // 验证网站属性及其验证器
        Assert.NotNull(properties["Website"]);
        Assert.Equal("string", properties["Website"]?["type"]?.ToString());
        Assert.Equal("Input", properties["Website"]?["x-component"]?.ToString());
        Assert.Equal("url", properties["Website"]?["x-validator"]?.ToString());
        Assert.Equal("uri", properties["Website"]?["format"]?.ToString());

        // 验证开关属性及其默认值
        Assert.NotNull(properties["IsActive"]);
        Assert.Equal("boolean", properties["IsActive"]?["type"]?.ToString());
        Assert.Equal("Switch", properties["IsActive"]?["x-component"]?.ToString());
        Assert.True(properties["IsActive"]?["default"]?.GetValue<bool>());

        // 验证分数属性及其默认值
        Assert.NotNull(properties["Score"]);
        Assert.Equal("number", properties["Score"]?["type"]?.ToString());
        Assert.Equal("NumberPicker", properties["Score"]?["x-component"]?.ToString());
        Assert.Equal(0.0, properties["Score"]?["default"]?.GetValue<double>());
    }

    #endregion

    #region XRenderSchemaGenerator

    [Fact]
    public void XRenderSchemaGenerator_Should_Register_Successfully()
    {
        // 注册自定义的 XRenderSchemaGenerator
        var services = new ServiceCollection();

        // 注册内存数据库
        services.AddDbContext<MyDbcontext>(options => options.UseInMemoryDatabase("TestDb"));

        services.AddBiwenContents<MyDbcontext>();

        services.AddSingleton<IContentSchemaGenerator, XRenderSchemaGenerator>();
        var serviceProvider = services.BuildServiceProvider();

        // 执行
        var schemaGenerator = serviceProvider.GetService<IContentSchemaGenerator>();

        // 断言
        Assert.NotNull(schemaGenerator);
        Assert.IsType<XRenderSchemaGenerator>(schemaGenerator);
    }

    [Fact]
    public void XRender_GenerateSchema_Should_Return_Valid_JsonObject_For_BlogType()
    {
        // 安排
        var schemaGenerator = new XRenderSchemaGenerator(GetMemoryCache());

        // 执行
        var schema = schemaGenerator.GenerateSchema<Blog>();

        // 输出生成的JSON以便检查
        _output.WriteLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));

        // 断言
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
        Assert.NotNull(schema["properties"]);

        var properties = schema["properties"] as JsonObject;
        Assert.NotNull(properties);

        // 验证标题属性
        Assert.NotNull(properties["Title"]);
        Assert.Equal("string", properties["Title"]?["type"]?.ToString());
        Assert.Equal("input", properties["Title"]?["widget"]?.ToString());
        Assert.Equal("FormItem", properties["Title"]?["x-decorator"]?.ToString());
        Assert.Equal(500, properties["Title"]?["maxLength"]?.GetValue<int>());

        // 验证发布状态属性
        Assert.NotNull(properties["IsPublished"]);
        Assert.Equal("boolean", properties["IsPublished"]?["type"]?.ToString());
        Assert.Equal("switch", properties["IsPublished"]?["widget"]?.ToString());

        // 验证描述属性
        Assert.NotNull(properties["Description"]);
        Assert.Equal("string", properties["Description"]?["type"]?.ToString());
        Assert.Equal("textArea", properties["Description"]?["widget"]?.ToString());
        Assert.Equal(500, properties["Description"]?["maxLength"]?.GetValue<int>());

        // 验证内容属性
        Assert.NotNull(properties["Content"]);
        Assert.Equal("string", properties["Content"]?["type"]?.ToString());
        Assert.Equal("markdown", properties["Content"]?["widget"]?.ToString());

        // 验证标签属性
        Assert.NotNull(properties["Tags"]);
        Assert.Equal("array", properties["Tags"]?["type"]?.ToString());
        Assert.Equal("tags", properties["Tags"]?["widget"]?.ToString());
        Assert.NotNull(properties["Tags"]?["items"]);
        Assert.Equal("string", properties["Tags"]?["items"]?["type"]?.ToString());
        Assert.Equal("input", properties["Tags"]?["items"]?["widget"]?.ToString());
        Assert.Equal("Input", properties["Tags"]?["items"]?["x-component"]?.ToString());

        // 验证必填字段
        Assert.NotNull(schema["required"]);
        var required = schema["required"] as JsonArray;
        Assert.NotNull(required);
        Assert.Contains("Title", required.Select(x => x?.ToString()).ToList());
        Assert.Contains("IsPublished", required.Select(x => x?.ToString()).ToList());
        Assert.Contains("Tags", required.Select(x => x?.ToString()).ToList());

        // 验证必填字段的required属性
        Assert.True(properties["Title"]?["required"]?.GetValue<bool>());
        Assert.True(properties["IsPublished"]?["required"]?.GetValue<bool>());
        Assert.True(properties["Tags"]?["required"]?.GetValue<bool>());
    }

    [Fact]
    public void XRender_GenerateSchemaJson_Should_Return_Valid_Json_String()
    {
        // 安排
        var schemaGenerator = new XRenderSchemaGenerator(GetMemoryCache());

        // 执行
        var schemaJson = schemaGenerator.GenerateSchemaJson<Blog>();

        // 输出生成的JSON以便检查
        _output.WriteLine(schemaJson);

        // 断言
        Assert.NotNull(schemaJson);
        Assert.True(schemaJson.Length > 0);

        // 尝试反序列化以验证JSON有效性
        var exception = Record.Exception(() => JsonDocument.Parse(schemaJson));
        Assert.Null(exception);
    }

    [Fact]
    public void XRender_GenerateSchema_Should_Throw_Exception_For_NonIContentType()
    {
        // 安排
        var schemaGenerator = new XRenderSchemaGenerator(GetMemoryCache());

        // 执行和断言
        var exception = Assert.Throws<ArgumentException>(() => schemaGenerator.GenerateSchema(typeof(NonIContentClass)));
        Assert.Contains("必须实现 IContent 接口", exception.Message);
    }

    [Fact]
    public void XRender_GenerateSchema_Should_Handle_CustomContent()
    {
        // 安排
        var schemaGenerator = new XRenderSchemaGenerator(GetMemoryCache());

        // 执行
        var schema = schemaGenerator.GenerateSchema<CustomContent>();
        var schemaJson = schemaGenerator.GenerateSchemaJson<CustomContent>();

        _output.WriteLine(schemaJson);

        // 断言
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());

        var properties = schema["properties"] as JsonObject;
        Assert.NotNull(properties);

        // 验证基本属性
        Assert.NotNull(properties["Name"]);
        Assert.Equal("string", properties["Name"]?["type"]?.ToString());
        Assert.Equal("input", properties["Name"]?["widget"]?.ToString());
        Assert.Equal(100, properties["Name"]?["maxLength"]?.GetValue<int>());

        // 验证年龄属性及其范围验证
        Assert.NotNull(properties["Age"]);
        Assert.Equal("number", properties["Age"]?["type"]?.ToString());
        Assert.Equal("inputNumber", properties["Age"]?["widget"]?.ToString());
        Assert.Equal(1.0, properties["Age"]?["minimum"]?.GetValue<double>());
        Assert.Equal(120.0, properties["Age"]?["maximum"]?.GetValue<double>());
        Assert.Equal(1.0, properties["Age"]?["min"]?.GetValue<double>());
        Assert.Equal(120.0, properties["Age"]?["max"]?.GetValue<double>());

        // 验证简介属性
        Assert.NotNull(properties["Bio"]);
        Assert.Equal("string", properties["Bio"]?["type"]?.ToString());
        Assert.Equal("textArea", properties["Bio"]?["widget"]?.ToString());
        Assert.Equal(1000, properties["Bio"]?["maxLength"]?.GetValue<int>());

        // 验证电话属性及其验证器
        Assert.NotNull(properties["Phone"]);
        Assert.Equal("string", properties["Phone"]?["type"]?.ToString());
        Assert.Equal("input", properties["Phone"]?["widget"]?.ToString());
        Assert.Equal("FormItem", properties["Phone"]?["x-decorator"]?.ToString());
        Assert.Equal("phone", properties["Phone"]?["x-validator"]?.ToString());
        Assert.Equal("tel", properties["Phone"]?["format"]?.ToString());

        // 验证邮箱属性及其验证器
        Assert.NotNull(properties["Email"]);
        Assert.Equal("string", properties["Email"]?["type"]?.ToString());
        Assert.Equal("input", properties["Email"]?["widget"]?.ToString());
        Assert.Equal("email", properties["Email"]?["format"]?.ToString());

        // 验证网站属性及其验证器
        Assert.NotNull(properties["Website"]);
        Assert.Equal("string", properties["Website"]?["type"]?.ToString());
        Assert.Equal("input", properties["Website"]?["widget"]?.ToString());
        Assert.Equal("url", properties["Website"]?["format"]?.ToString());

        // 验证开关属性及其默认值
        Assert.NotNull(properties["IsActive"]);
        Assert.Equal("boolean", properties["IsActive"]?["type"]?.ToString());
        Assert.Equal("switch", properties["IsActive"]?["widget"]?.ToString());
        Assert.True(properties["IsActive"]?["default"]?.GetValue<bool>());

        // 验证分数属性及其默认值
        Assert.NotNull(properties["Score"]);
        Assert.Equal("number", properties["Score"]?["type"]?.ToString());
        Assert.Equal("inputNumber", properties["Score"]?["widget"]?.ToString());
        Assert.Equal(0.0, properties["Score"]?["default"]?.GetValue<double>());

        // 验证必填字段
        Assert.NotNull(schema["required"]);
        var required = schema["required"] as JsonArray;
        Assert.NotNull(required);
        Assert.Contains("Name", required.Select(x => x?.ToString()).ToList());
        Assert.True(properties["Name"]?["required"]?.GetValue<bool>());
    }


    [Fact]
    public void XRender_Should_Handle_Number_Fields_With_Both_Min_Max_And_Minimum_Maximum()
    {
        // 安排
        var schemaGenerator = new XRenderSchemaGenerator(GetMemoryCache());

        // 执行
        var schema = schemaGenerator.GenerateSchema<CustomContent>();

        // 验证
        var properties = schema["properties"] as JsonObject;
        Assert.NotNull(properties);

        // 验证Age属性同时包含min/max和minimum/maximum属性
        var ageProp = properties["Age"] as JsonObject;
        Assert.NotNull(ageProp);

        Assert.NotNull(ageProp["minimum"]);
        Assert.NotNull(ageProp["maximum"]);
        Assert.NotNull(ageProp["min"]);
        Assert.NotNull(ageProp["max"]);

        Assert.Equal(ageProp["minimum"]?.GetValue<double>(), ageProp["min"]?.GetValue<double>());
        Assert.Equal(ageProp["maximum"]?.GetValue<double>(), ageProp["max"]?.GetValue<double>());

    }

    #endregion



    // 测试用的非IContent类
    protected class NonIContentClass
    {
        public string Name { get; set; } = string.Empty;
    }

    // 测试用的自定义IContent实现
    protected class CustomContent : ContentBase<CustomContent>
    {
        [Required, StringLength(100)]
        public TextFieldType Name { get; set; } = null!;

        [Range(1, 120)]
        public IntegerFieldType Age { get; set; } = null!;

        [StringLength(1000)]
        public TextAreaFieldType Bio { get; set; } = null!;

        [Phone]
        [RegularExpression("^\\+?[1-9]\\d{1,14}$", ErrorMessage = "Invalid phone number format.")]
        public TextFieldType Phone { get; set; } = null!;

        [EmailAddress]
        public TextFieldType Email { get; set; } = null!;

        [Url]
        public UrlFieldType Website { get; set; } = null!;


        [DefaultValue(true)]
        public BooleanFieldType IsActive { get; set; } = null!;

        [Display(Name = "分数")]
        [DefaultValue(0)]
        public NumberFieldType Score { get; set; } = null!;

        /// <summary>
        /// 上传文件测试
        /// </summary>
        public FileFieldType File { get; set; } = null!;


        /// <summary>
        /// 颜色字段
        /// </summary>
        [Display(Name = "颜色")]
        public ColorFieldType? Color { get; set; }


        [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm")]
        public DateTimeFieldType? CreatedAt { get; set; }

        [DisplayFormat(DataFormatString = "yyyy-MM-dd")]
        public DateTimeFieldType? CreatedAt2 { get; set; }

        /// <summary>
        /// 模拟单选字段
        /// </summary>
        [Display(Name = "选择")]
        [DefaultValue(CustomEnum.Option1)]
        public OptionsFieldType<CustomEnum> OptionsSelect { get; set; } = null!;

        /// <summary>
        /// 模拟多选字段
        /// </summary>
        [Display(Name = "多选")]
        [Required, DefaultValue(CustomEnum.Option1)]
        public OptionsMultiFieldType<CustomEnum> ChecksSelect { get; set; } = null!;


        [Display(Name = "多行文本")]
        [DefaultValue("hello world")]
        public TextAreaFieldType Description { get; set; } = null!;

    }

    [Description("自定义枚举")]
    protected enum CustomEnum
    {
        [Description("选项1")]
        Option1 = 0,
        [Description("选项2")]
        Option2 = 1,
        [Description("选项3")]
        Option3 = 2
    }


}


/// <summary>
/// 模拟的DbContext
/// </summary>
public class MyDbcontext : DbContext, IContentDbContext
{
    public MyDbcontext(DbContextOptions<MyDbcontext> options) : base(options)
    {

    }

    public DbSet<Content> Contents { get; set; } = null!;
    public DbSet<ContentAuditLog> ContentAuditLogs { get; set; }
    public DbSet<ContentVersion> ContentVersions { get; set; }

    public DbContext Context => this;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Content>().ToTable("Contents");
    }
}

