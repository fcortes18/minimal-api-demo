using MinimalAPI.Auth;
using MinimalAPI.DataSource.Tables;
using Newtonsoft.Json;
using System.Text;

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
        public async Task CreateShoppingCartItem_ShouldReturnItemCreated()
        {
            // generate token
            using var tokenResponse = await _client.PostAsync("/login", null);
            Assert.True(tokenResponse.IsSuccessStatusCode);
            var tokenStringResponse = await tokenResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(tokenStringResponse));
            var token = JsonConvert.DeserializeObject<Token>(tokenStringResponse);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token!.AccessToken}");

            // create shopping cart item
            ShoppingCartItem shoppingItem = new()
            {
                IsPickedUp = false,
                ItemName = "Test item",
                Quantity = 1
            };
            using var createResponse = await _client.PostAsync("/shoppingcart", new StringContent(JsonConvert.SerializeObject(shoppingItem), Encoding.UTF8, "application/json"));
            Assert.True(createResponse.IsSuccessStatusCode);
            var createStringResponse = await createResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(createStringResponse));
            var createdShoppingCartItem = JsonConvert.DeserializeObject<ShoppingCartItem>(createStringResponse);

            // get shopping cart item created
            using var response = await _client.GetAsync($"/shoppingcart/{createdShoppingCartItem!.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var getStringResponse = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(getStringResponse));
            var shoppingCartItem = JsonConvert.DeserializeObject<ShoppingCartItem>(getStringResponse);
            Assert.True(createdShoppingCartItem!.Id == shoppingCartItem.Id);
        }
    }
}
