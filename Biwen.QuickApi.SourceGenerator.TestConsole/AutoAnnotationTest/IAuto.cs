
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.SourceGenerator.TestConsole.AutoAnnotationTest
{
    [AutoAnnotation]

    public interface IAuto
    {

        [Description("hello")]
        [DefaultValue("hello")]
        string? Hello { get; set; }

        [Description("world")]
        [DefaultValue("world"), Required, StringLength(50, MinimumLength = 2)]
        string? World { get; set; }

        void Say();



    }

    public partial class AutoClass : IAuto
    {
        public void Say()
        {
            Console.WriteLine($"{Hello} {World}");
        }
    }
}