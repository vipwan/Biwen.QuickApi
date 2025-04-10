// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: 万雅虎, Github: https://github.com/vipwan
// 
// Modify Date: 2025-04-07 22:01:23 ContentValidatorTest.cs

using Biwen.QuickApi.Contents;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using Biwen.QuickApi.DemoWeb.Cms;
using System.ComponentModel.DataAnnotations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Biwen.QuickApi.Test.Contents;

public class ContentValidatorTest
{
    private readonly ITestOutputHelper _output;

    public ContentValidatorTest(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Blog验证测试

    [Fact]
    public async Task ValidateAsync_With_Valid_Blog_Should_Return_Success()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            Title = new TextFieldType("有效的博客标题"),
            IsPublished = new BooleanFieldType(true),
            Description = new TextAreaFieldType("这是博客描述"),
            Content = new MarkdownFieldType("# 这是Markdown内容"),
            Tags = new ArrayFieldType(["标签1,标签2"]),
        };
        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public async Task ValidateAsync_With_Empty_Title_Should_Return_Error()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            Title = new TextFieldType(""),  // 空标题，应该验证失败
            IsPublished = new BooleanFieldType(true),
            Description = new TextAreaFieldType("这是博客描述"),
            Content = new MarkdownFieldType("# 这是Markdown内容"),
            Tags = new ArrayFieldType(["标签1,标签2"])
        };

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Title", result!.MemberNames);
        Assert.Contains("不能为空", result.ErrorMessage);
        _output.WriteLine($"验证错误信息: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ValidateAsync_With_Title_Exceeding_MaxLength_Should_Return_Error()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            // 创建一个超过500字符的标题
            Title = new TextFieldType { Value = new string('a', 501) },
            IsPublished = new BooleanFieldType(true),
            Description = new TextAreaFieldType("这是博客描述"),
            Content = new MarkdownFieldType("# 这是Markdown内容"),
            Tags = new ArrayFieldType(["标签1,标签2"])
        };

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Title", result!.MemberNames);
        Assert.Contains("长度", result.ErrorMessage);
        _output.WriteLine($"验证错误信息: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ValidateAsync_With_Empty_Tags_Should_Return_Error()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            Title = new TextFieldType { Value = "有效的博客标题" },
            IsPublished = new BooleanFieldType { Value = "true" },
            Description = new TextAreaFieldType { Value = "这是博客描述" },
            Content = new MarkdownFieldType { Value = "# 这是Markdown内容" },
            Tags = new ArrayFieldType { Value = "" }  // 空标签，应该验证失败
        };

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Tags", result!.MemberNames);
        Assert.Contains("不能为空", result.ErrorMessage);
        _output.WriteLine($"验证错误信息: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ValidateAsync_With_Null_Content_Should_Pass()
    {
        // 准备 - Content 没有 Required 标记，可以为空
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            Title = new TextFieldType { Value = "有效的博客标题" },
            IsPublished = new BooleanFieldType { Value = "true" },
            Description = new TextAreaFieldType { Value = "这是博客描述" },
            Content = new MarkdownFieldType { Value = null! },  // 空内容
            Tags = new ArrayFieldType { Value = "标签1,标签2" }
        };

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public async Task ValidateAsync_With_Invalid_Boolean_Value_Should_Return_Error()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            Title = new TextFieldType { Value = "有效的博客标题" },
            IsPublished = new BooleanFieldType { Value = "invalid" },  // 无效的布尔值
            Description = new TextAreaFieldType { Value = "这是博客描述" },
            Content = new MarkdownFieldType { Value = "# 这是Markdown内容" },
            Tags = new ArrayFieldType { Value = "标签1,标签2" }
        };

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("IsPublished", result!.MemberNames);
        _output.WriteLine($"验证错误信息: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ValidateAsync_With_Long_Description_Should_Return_Error()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var blog = new Blog
        {
            Title = new TextFieldType { Value = "有效的博客标题" },
            IsPublished = new BooleanFieldType { Value = "true" },
            Description = new TextAreaFieldType { Value = new string('a', 501) },  // 超过500字符的描述
            Content = new MarkdownFieldType { Value = "# 这是Markdown内容" },
            Tags = new ArrayFieldType { Value = "标签1,标签2" }
        };

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Description", result!.MemberNames);
        Assert.Contains("长度", result.ErrorMessage);
        _output.WriteLine($"验证错误信息: {result.ErrorMessage}");
    }

    #endregion

    #region 自定义内容类型测试

    [Fact]
    public async Task ValidateAsync_With_CustomContent_Should_Validate_All_Fields()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var customContent = new TestCustomContent
        {
            Name = new TextFieldType { Value = "测试名称" },
            Email = new TextFieldType { Value = "not-an-email" },  // 无效的电子邮件
            Age = new IntegerFieldType { Value = "200" }  // 超出范围的年龄
        };

        // 执行
        var result = await validator.ValidateAsync(customContent);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        _output.WriteLine($"验证错误信息: {result!.ErrorMessage}");

        // 确认是Email或Age中的一个验证失败
        Assert.True(result.MemberNames.Contains("Email") || result.MemberNames.Contains("Age"));
    }

    [Fact]
    public async Task ValidateAsync_With_Valid_CustomContent_Should_Pass()
    {
        // 准备
        var validator = new DefaultContentValidator();
        var customContent = new TestCustomContent
        {
            Name = new TextFieldType { Value = "测试名称" },
            Email = new TextFieldType { Value = "test@example.com" },  // 有效的电子邮件
            Age = new IntegerFieldType { Value = "30" }  // 有效的年龄
        };

        // 执行
        var result = await validator.ValidateAsync(customContent);

        // 断言
        Assert.Equal(ValidationResult.Success, result);
    }

    #endregion

    #region 边界条件测试

    [Fact]
    public async Task ValidateAsync_With_Null_Content_Should_Return_Error()
    {
        // 准备
        var validator = new DefaultContentValidator();
        Blog blog = null!;

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("内容对象不能为空", result!.ErrorMessage);
        _output.WriteLine($"验证错误信息: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ValidateAsync_With_Empty_Object_Should_Return_Errors()
    {
        // 准备 - 所有必填字段都为空
        var validator = new DefaultContentValidator();
        var blog = new Blog();  // 默认构造的对象，所有字段都是null

        // 执行
        var result = await validator.ValidateAsync(blog);

        // 断言
        Assert.NotEqual(ValidationResult.Success, result);
        _output.WriteLine($"验证错误信息: {result!.ErrorMessage}");
    }

    #endregion

    #region 辅助测试类

    /// <summary>
    /// 用于测试的自定义内容类型
    /// </summary>
    private class TestCustomContent : ContentBase<TestCustomContent>
    {
        [Required, StringLength(100)]
        public TextFieldType Name { get; set; } = null!;

        [EmailAddress]
        public TextFieldType Email { get; set; } = null!;

        [Range(1, 120)]
        public IntegerFieldType Age { get; set; } = null!;
    }

    #endregion
}
