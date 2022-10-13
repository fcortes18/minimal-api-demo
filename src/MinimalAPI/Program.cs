using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPI.Auth;
using MinimalAPI.DataSource.Tables;
using MinimalAPI.Swagger;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token only",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, Array.Empty<string>()}
    });
    c.ParameterFilter<GuidParameterFilter>();
});

// configure strongly typed TokenOptions object
var tokenOptionsSection = builder.Configuration.GetSection(nameof(TokenOptions));
var tokenOptions = tokenOptionsSection.Get<TokenOptions>();
builder.Services.Configure<TokenOptions>(tokenOptionsSection);
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = tokenOptions?.Issuer,
        ValidAudience = tokenOptions?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions?.SecurityKey ?? string.Empty))
    };
});

builder.Services.AddCors(options => 
    {
        options.AddPolicy("AllowAll", a => a.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
    }
);

builder.Services.AddSingleton<ITokenHelper, JwtTokenHelper>();
builder.Services.AddDbContext<StoreDbContext>(opt => opt.UseSqlServer("CONNECTION_STRING"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

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
.AllowAnonymous();

// https://github.com/dotnet/aspnetcore/issues/39886
app.MapGet("/shoppingcart/{id:guid}", async (Guid id, StoreDbContext db) =>
{
    var item = await db.ShoppingCarts.Where(s => s.Id == id).FirstOrDefaultAsync();
    return item == null ? Results.NotFound() : Results.Ok(item);
})
.RequireCors("AllowAll")
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status200OK, typeof(ShoppingCartItem))
.Produces(StatusCodes.Status404NotFound)
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
.RequireAuthorization();

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

internal record FileModel(IFormFile? File)
{
    public static async ValueTask<FileModel?> BindAsync(HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();
        var file = form.Files["file"];
        return new FileModel(file);
    }
}
