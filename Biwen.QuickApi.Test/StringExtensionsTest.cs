namespace Biwen.QuickApi.Test
{
    public class StringExtensionsTest(ITestOutputHelper testOutput)
    {
        [Theory]
        [InlineData('a', true)]
        [InlineData('A', true)]
        [InlineData('0', false)]
        [InlineData('我', false)]
        public void IsLetterTest(char letter, bool flag)
        {
            testOutput.WriteLine($"IsLetterTest: {letter} => {flag}");
            Assert.Equal(flag, letter.IsLetter());
        }

        //测试是否空格
        [Theory]
        [InlineData(' ', true)]
        [InlineData('\r', true)]
        [InlineData('\n', true)]
        [InlineData('\t', true)]
        [InlineData('\f', true)]
        [InlineData('a', false)]
        public void IsSpaceTest(char letter, bool flag)
        {
            testOutput.WriteLine($"IsSpaceTest: {letter} => {flag}");
            Assert.Equal(flag, letter.IsSpace());
        }

        //RemoveTags test
        [Theory]
        [InlineData("<p>hello</p>", "hello")]
        [InlineData("<p>hello</p><p>world</p>", "helloworld")]
        [InlineData("<p>hello</p><p>world</p><p>!</p>", "helloworld!")]
        [InlineData("<p>hello</p><p>world</p><p>!</p><p> </p>", "helloworld! ")]
        public void RemoveTagsTest(string html, string result)
        {
            testOutput.WriteLine($"RemoveTagsTest: {html} => {result}");
            Assert.Equal(result, html.RemoveTags());
        }

        //ToPascalCase test
        [Theory]
        [InlineData("hello", "Hello", ' ')]
        [InlineData("hello world", "HelloWorld", ' ')]
        [InlineData("hello-world", "HelloWorld", '-')]
        [InlineData("hello_world", "HelloWorld", '_')]
        [InlineData("hello_world_", "HelloWorld", '_')]
        public void ToPascalCaseTest(string text, string result, char upperAfterDelimiter)
        {
            testOutput.WriteLine($"ToPascalCaseTest: {text} => {result}");
            Assert.Equal(result, text.ToPascalCase(upperAfterDelimiter));
        }


        //ToCamalCase test
        [Theory]
        [InlineData(" hello ", "hello")]
        [InlineData("hello             world", "helloWorld")]
        [InlineData("hello world      two", "helloWorldTwo")]
        public void ToCamalCaseTest(string text, string result)
        {
            testOutput.WriteLine($"ToCamalCaseTest: {text} => {result}");
            Assert.Equal(result, text.ToCamalCase());
        }

        //IsJson test
        [Theory]
        [InlineData("{\"name\":\"biwen\"}", true)]
        [InlineData("hello world", false)]
        [InlineData("[]", true)]
        [InlineData("", false)]
        public void IsJsonTest(string text, bool flag)
        {
            testOutput.WriteLine($"IsJsonTest: {text} => {flag}");
            Assert.Equal(flag, text.IsJson());
        }


        //ToMD5 test
        [Fact]
        public void ToMD5Test()
        {
            var text = "hello";
            var result = "5d41402abc4b2a76b9719d911017c592";
            testOutput.WriteLine($"ToMD5Test: {text} => {result}");
            Assert.Equal(result, text.ToMD5(false));
        }
    }

}