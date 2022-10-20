using MinimalAPI.Auth;
using MinimalAPI.DataSource.Tables;

namespace MinimalAPI.IntegrationTests
{
    public class ShoppingCartTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ShoppingCartTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ShoppingCartItem_LoginPostGet_MatchId()
        {
            // login
            using var tokenResponse = await _client
                .PostAsJsonAsync<StringContent>("/login", null);
            var token = await tokenResponse.Content.ReadFromJsonAsync<Token>();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token!.AccessToken}");

            // create 
            ShoppingCartItem shoppingItem = new()
            {
                IsPickedUp = false,
                ItemName = "Test item",
                Quantity = 1
            };
            using var createResponse = await _client
                .PostAsJsonAsync("/shoppingcart", shoppingItem);
            var created = await createResponse.Content.ReadFromJsonAsync<ShoppingCartItem>();
            
            // read created
            var read = await _client
                .GetFromJsonAsync<ShoppingCartItem>($"/shoppingcart/{created!.Id}");
            
            // assert
            Assert.NotNull(read);
            Assert.True(created.Id == read.Id);
        }
    }
}
