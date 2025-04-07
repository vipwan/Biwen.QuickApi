using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.DemoWeb.Cms;

public class Blog : ContentBase<Blog>
{
    [Required, StringLength(500)]
    public TextFieldType Title { get; set; } = null!;

    [Required, DefaultValue(true)]
    public BooleanFieldType IsPublished { get; set; } = null!;

    [StringLength(500)]
    public TextAreaFieldType Description { get; set; } = null!;

    public MarkdownFieldType Content { get; set; } = null!;

    [Required]
    public ArrayFieldType Tags { get; set; } = null!;

}
