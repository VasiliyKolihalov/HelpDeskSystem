using Authentication.WebApi.Models.Http.Users;

namespace Authentication.WebApi.Clients.Users;

public interface IUsersClient
{
    public Task<Guid> SendCreateRequestAsync(UserCreate userCreate, string jwt);
    public Task SendUserRequestAsync(Guid userId, string jwt);
}