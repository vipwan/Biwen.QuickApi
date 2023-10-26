using Microsoft.AspNetCore.Mvc.Testing;

namespace Biwen.QuickApi.Test
{
    using TestSite = DemoWeb;


    public class AntiUnitTest
    {

        static HttpClient TestClient { get; } = new WebApplicationFactory<TestSite.Program>().CreateClient();


        [Fact]
        public async Task Login()
        {
            var response = await TestClient.GetAsync("/quick/admin/logined");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("已经登录成功", content);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task LoginOut()
        {
            var response = await TestClient.GetAsync("/quick/admin/loginout");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("已经退出登录", content);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Auth()
        {
            var response = await TestClient.GetAsync("/quick/admin/an-auth");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
           //Assert.True(response.IsSuccessStatusCode);
        }


        [Fact]
        public async Task Middleware()
        {
            var response = await TestClient.GetAsync("/quick/middleware");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("middleware", content);
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}