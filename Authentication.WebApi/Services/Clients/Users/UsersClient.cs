using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using Authentication.WebApi.Constants;
using Authentication.WebApi.Models.Http.Users;
using static Authentication.WebApi.Constants.PermissionNames;

namespace Authentication.WebApi.Services.Clients.Users;

public class UsersClient : IUsersClient
{
    private readonly HttpClient _httpClient;
    private readonly IJwtService _jwtService;

    public UsersClient(IHttpClientFactory factory, IJwtService jwtService)
    {
        _jwtService = jwtService;
        _httpClient = factory.CreateClient(HttpClientNames.Users);
    }

    public async Task<UserView> SendGetRequestAsync(Guid userId)
    {
        var userView = (await _httpClient.GetFromJsonAsync<UserView>($"users/{userId}"))!;
        return userView;
    }

    public async Task<Guid> SendPostRequestAsync(UserCreate userCreate)
    {
        string jwt = _jwtService.GenerateJwt(new Account<Guid>
        {
            Permissions = new[] { new Permission { Id = HttpClientPermissions.UsersCreate } }
        });
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);
        HttpResponseMessage result = await _httpClient.PostAsJsonAsync("users", userCreate);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task SendDeleteRequestAsync(Guid userId)
    {
        string jwt = _jwtService.GenerateJwt(new Account<Guid>
        {
            Permissions = new[] { new Permission { Id = HttpClientPermissions.UsersDelete } }
        });
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);
        HttpResponseMessage result = await _httpClient.DeleteAsync($"users/{userId}");
        result.EnsureSuccessStatusCode();
    }
}