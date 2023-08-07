using Authentication.Infrastructure.Extensions;
using Authentication.WebApi.Constants;
using Authentication.WebApi.Models.Http.Users;
namespace Authentication.WebApi.Services.Clients.Users;

public class UsersClient : IUsersClient
{
    private readonly HttpClient _httpClient;

    public UsersClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient(HttpClientNames.Users);
    }

    public async Task<UserView> SendGetRequestAsync(Guid userId)
    {
        var userView = (await _httpClient.GetFromJsonAsync<UserView>($"users/{userId}"))!;
        return userView;
    }

    public async Task<Guid> SendPostRequestAsync(UserCreate userCreate, string jwt)
    {
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);
        HttpResponseMessage result = await _httpClient.PostAsJsonAsync("users", userCreate);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task SendDeleteRequestAsync(Guid userId, string jwt)
    {
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);
        HttpResponseMessage result = await _httpClient.DeleteAsync($"users/{userId}");
        result.EnsureSuccessStatusCode();
    }
}