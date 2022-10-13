namespace MinimalAPI.Auth
{
    public record Token(string AccessToken, DateTime Expiration);
}
