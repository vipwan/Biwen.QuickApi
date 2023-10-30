using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Biwen.QuickApi.SourceGenerator.TestConsole.AutoAnnotationTest
{
    public partial class AutoClass
    {
        [Description("hello")]
        [DefaultValue("hello")]
        public virtual string? Hello { get; set; }

        [Description("world")]
        [DefaultValue("world"), Required, StringLength(50, MinimumLength = 2)]
        public virtual string? World { get; set; }


    }



}