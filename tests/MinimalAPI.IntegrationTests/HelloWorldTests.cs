using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace MinimalAPI.IntegrationTests
{
    public class HelloWorldTests
    {
        [Fact]
        public async Task TestRootEndpoint()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            var response = await client.GetAsync("/");
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(responseText));
            Assert.Equal("Hello world!", responseText);
        }
    }
}
