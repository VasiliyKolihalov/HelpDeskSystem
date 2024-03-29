using Authentication.Api.Models.Http.Users;

namespace Authentication.Api.Services.Clients.Users;

public interface IUsersClient
{
    public Task<UserView> SendGetRequestAsync(Guid userId);
    public Task<Guid> SendPostRequestAsync(UserCreate userCreate);
    public Task SendDeleteRequestAsync(Guid userId);
}