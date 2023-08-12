using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using SupportTickets.WebApi.Constants;
using static SupportTickets.WebApi.Constants.PermissionNames;

namespace SupportTickets.WebApi.Services.Clients;

public class AccountsClient : IAccountsClient
{
    private readonly HttpClient _httpClient;
    private readonly IJwtService _jwtService;

    public AccountsClient(IHttpClientFactory httpClientFactory, IJwtService jwtService)
    {
        _jwtService = jwtService;
        _httpClient = httpClientFactory.CreateClient(HttpClientNames.Accounts);
    }

    public async Task<Account<Guid>> SendGetRequestAsync(Guid accountId)
    {
        string jwt = _jwtService.GenerateJwt(new Account<Guid>
        {
            Permissions = new[] { new Permission { Id = HttpClientPermissions.AccountsGetById } }
        });
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);
        var account = (await _httpClient.GetFromJsonAsync<Account<Guid>>($"accounts/{accountId}"))!;
        return account;
    }
}