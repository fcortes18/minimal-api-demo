using Microsoft.EntityFrameworkCore;
using MinimalAPI;
using MinimalAPI.Auth;
using MinimalAPI.DataSource.Tables;
using MinimalAPI.Entities;
using MinimalAPI.Extensions;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiServicesSetup(builder.Configuration);

var app = builder.Build();
app.AddApiAppSetup();

app.MapGet("/hello", () => "Hello world")
.GetEndpointProduces()
//.WithMetadata(new ApiKeyAttribute());
.RequireApiKey();

// https://github.com/dotnet/aspnetcore/issues/39886
app.MapGet("/shoppingcart/{id:guid}", async (Guid id, StoreDbContext db) =>
{
    var item = await db.ShoppingCarts.Where(s => s.Id == id).FirstOrDefaultAsync();
    return item == null ? Results.NotFound() : Results.Ok(item);
})
.RequireCors("AllowAll")
.GetEndpointProduces(typeof(ShoppingCartItem))
.RequireAuthorization();

app.MapGet("/shoppingcart", async (StoreDbContext db) =>
{
    var items = await db.ShoppingCarts.ToListAsync();
    return items.Any() ? Results.Ok(items) : Results.NotFound();
})
.RequireCors("AllowAll")
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status200OK, typeof(IEnumerable<ShoppingCartItem>))
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError)
.RequireAuthorization();

app.MapPost("/login", (ITokenHelper _tokenHelper) =>
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        new(ClaimTypes.Name, "Test User"),
        new(ClaimTypes.Email, "minimal.api.demo@example.com"),
        new Claim(ClaimTypes.Role, "admin")
    };
    return _tokenHelper.GetAccessToken(claims);
})
.RequireCors("AllowAll")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError)
.AllowAnonymous();

app.MapPost("/shoppingcart", async (ShoppingCartItem shoppingCart, StoreDbContext db) =>
{
    await db.ShoppingCarts.AddAsync(shoppingCart);
    await db.SaveChangesAsync();
    return Results.Created("/shoppingcart", shoppingCart);
}
)
.RequireCors("AllowAll")
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status201Created, typeof(ShoppingCartItem))
.Produces(StatusCodes.Status400BadRequest)
.RequireAuthorization();

app.MapPost("/upload", (FileModel model) =>
{
    return model.File == null ? Results.BadRequest("No file was attached") : Results.Ok(new { fileName = model.File.FileName });
})
.RequireCors("AllowAll")
.Accepts<FileModel>("multipart/form-data")
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status200OK, typeof(IEnumerable<ShoppingCartItem>))
.Produces(StatusCodes.Status400BadRequest)
.AllowAnonymous();

app.Run();

public partial class Program { }