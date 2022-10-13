using Asp.Versioning;
using Asp.Versioning.Conventions;
using MinimalAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    }
).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = false;
    }
);

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var versionSet = app.NewApiVersionSet()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .HasApiVersion(new ApiVersion(2, 0))
                    .ReportApiVersions()
                    .Build();

var galleryVersionSet = app.NewApiVersionSet("Upload")
                        .Build();

app.UseHttpsRedirection();

// 1.0
app.MapGet("/api/hello", () => "Hello world!")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0);

// 2.0
app.MapGet("/api/hello", () => "Hello world 2!")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(2.0);

app.MapPost("/api/upload", (FileModel model) =>
{
    return model.File == null ? Results.BadRequest("No file was attached") : Results.Ok(new { fileName = model.File.FileName });
})
.Accepts<FileModel>("multipart/form-data")
.WithApiVersionSet(galleryVersionSet)
.HasApiVersion(1.0);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

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