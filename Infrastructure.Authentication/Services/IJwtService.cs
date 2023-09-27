using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Services;

public interface IJwtService
{
    public string GenerateJwt<TId>(Account<TId> account) where TId : notnull;
}