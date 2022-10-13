using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Auth;
using MinimalAPI.DataSource.Tables;
using Newtonsoft.Json;
using System.Text;

namespace MinimalAPI.IntegrationTests
{
    public class ShoppingCartTests
    {
        [Fact]
        public async Task CreateShoppingCartItem_ShouldReturnItemCreated()
        {
            var appFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType ==
                            typeof(DbContextOptions<StoreDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<StoreDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestInMemory");
                        });
                    });
                });

            var httpClient = appFactory.CreateClient();

            // generate token
            using var tokenResponse = await httpClient.PostAsync("/login", null);
            Assert.True(tokenResponse.IsSuccessStatusCode);
            var tokenStringResponse = await tokenResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(tokenStringResponse));
            var token = JsonConvert.DeserializeObject<Token>(tokenStringResponse);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token!.AccessToken}");

            // create shopping cart item
            ShoppingCartItem shoppingItem = new()
            {
                IsPickedUp = false,
                ItemName = "Test item",
                Quantity = 1
            };
            using var createResponse = await httpClient.PostAsync("/shoppingcart", new StringContent(JsonConvert.SerializeObject(shoppingItem), Encoding.UTF8, "application/json"));
            Assert.True(createResponse.IsSuccessStatusCode);
            var createStringResponse = await createResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(createStringResponse));
            var createdShoppingCartItem = JsonConvert.DeserializeObject<ShoppingCartItem>(createStringResponse);

            // get shopping cart item created
            using var response = await httpClient.GetAsync($"/shoppingcart/{createdShoppingCartItem!.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var getStringResponse = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(getStringResponse));
            var shoppingCartItem = JsonConvert.DeserializeObject<ShoppingCartItem>(getStringResponse);
            Assert.True(createdShoppingCartItem!.Id == shoppingCartItem.Id);
        }
    }
}
