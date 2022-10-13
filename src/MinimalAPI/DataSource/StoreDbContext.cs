using Microsoft.EntityFrameworkCore;
namespace MinimalAPI.DataSource.Tables;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
    {
    }

    public DbSet<ShoppingCartItem> ShoppingCarts { get; set; }
}
