namespace SafeVaultApi.Services
{
    public class TokenService
    {
        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}