using Authentication.WebApi.Models.Http.Users;

namespace Authentication.WebApi.Clients.Users;

public interface IUsersClient
{
    public Task<Guid> SendPostRequestAsync(UserCreate userCreate, string jwt);
    public Task SendDeleteRequestAsync(Guid userId, string jwt);
}