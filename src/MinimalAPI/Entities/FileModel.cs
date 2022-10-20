namespace MinimalAPI.Entities;

internal record FileModel(IFormFile? File)
{
    public static async ValueTask<FileModel?> BindAsync(HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();
        var file = form.Files["file"];
        return new FileModel(file);
    }
}