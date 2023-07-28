using Authentication.Infrastructure.Models;

namespace Authentication.Infrastructure.Services;

public interface IJwtService
{
    public string GenerateJwt<TId>(Account<TId> account);
}