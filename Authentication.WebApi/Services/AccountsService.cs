using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using Authentication.WebApi.Models;
using Authentication.WebApi.Models.Accounts;
using Authentication.WebApi.Models.Http.Users;
using Authentication.WebApi.Repositories;
using AutoMapper;
using Infrastructure.Exceptions;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Authentication.WebApi.Services;

public class AccountsService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IJwtService _jwtService;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;

    public AccountsService(
        IAccountsRepository accountsRepository,
        IRolesRepository rolesRepository,
        IJwtService jwtService,
        IHttpClientFactory httpClientFactory,
        IMapper mapper)
    {
        _accountsRepository = accountsRepository;
        _rolesRepository = rolesRepository;
        _jwtService = jwtService;
        _httpClient = httpClientFactory.CreateClient("Users.WebApi");
        _mapper = mapper;
    }

    public async Task<string> RegisterAsync(UserAccountRegister userAccountRegister)
    {
        if (await _accountsRepository.IsExistsAsync(userAccountRegister.Email))
            throw new BadRequestException($"Account with email {userAccountRegister.Email} already exist");

        var userAccount = _mapper.Map<UserAccount>(userAccountRegister);
        userAccount.Id = await SendCreateUserRequest(_mapper.Map<UserCreate>(userAccountRegister));
        userAccount.PasswordHash = BCryptNet.HashPassword(userAccountRegister.Password);
        await _accountsRepository.InsertAsync(userAccount);

        return _jwtService.GenerateJwt(_mapper.Map<Account<Guid>>(userAccount));
    }

    private async Task<Guid> SendCreateUserRequest(UserCreate userCreate)
    {
        HttpResponseMessage result = await _httpClient.PostAsJsonAsync("users", userCreate);
        return await result.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task<string> LoginAsync(UserAccountLogin userAccountLogin)
    {
        UserAccount userAccount = await _accountsRepository.GetByEmailAsync(userAccountLogin.Email)
                                  ?? throw new BadRequestException(
                                      $"User with email {userAccountLogin.Email} dose not exist");

        if (!BCryptNet.Verify(text: userAccountLogin.Password, hash: userAccount.PasswordHash))
            throw new BadRequestException("Incorrect password");

        return _jwtService.GenerateJwt(_mapper.Map<Account<Guid>>(userAccount));
    }

    public async Task AddToRoleAsync(Guid toAccountId, string roleId)
    {
        UserAccount account = await _accountsRepository.GetByIdAsync(toAccountId)
                              ?? throw new NotFoundException($"Account with id {toAccountId} not found");

        if (account.Roles.Any(_ => _.Id == roleId))
            throw new BadRequestException($"Account with id {account.Id} already have role {roleId}");

        if (!await _rolesRepository.IsExistsAsync(roleId))
            throw new NotFoundException($"Role with id {roleId} not found");

        await _accountsRepository.AddRoleAsync(toAccountId, roleId);
    }

    public async Task RemoveFromRoleAsync(Guid toAccountId, string roleId)
    {
        UserAccount account = await _accountsRepository.GetByIdAsync(toAccountId)
                              ?? throw new NotFoundException($"Account with id {toAccountId} not found");

        if (account.Roles.All(_ => _.Id != roleId))
            throw new BadRequestException($"Account with id {account.Id} dose not have role {roleId}");

        await _accountsRepository.RemoveRoleAsync(toAccountId, roleId);
    }
}