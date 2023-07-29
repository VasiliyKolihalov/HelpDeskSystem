using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using Authentication.WebApi.Clients.Users;
using Authentication.WebApi.Models.Accounts;
using Authentication.WebApi.Models.Http.Users;
using Authentication.WebApi.Repositories;
using AutoMapper;
using Infrastructure.Exceptions;
using static Authentication.WebApi.Constants.PermissionNames;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Authentication.WebApi.Services;

public class AccountsService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IJwtService _jwtService;
    private readonly IUsersClient _usersClient;
    private readonly IMapper _mapper;

    public AccountsService(
        IAccountsRepository accountsRepository,
        IRolesRepository rolesRepository,
        IJwtService jwtService,
        IUsersClient usersClient,
        IMapper mapper)
    {
        _accountsRepository = accountsRepository;
        _rolesRepository = rolesRepository;
        _jwtService = jwtService;
        _usersClient = usersClient;
        _mapper = mapper;
    }

    public async Task<string> RegisterAsync(UserAccountRegister register)
    {
        if (await _accountsRepository.IsExistsAsync(register.Email))
            throw new BadRequestException($"Account with email {register.Email} already exist");

        var userAccount = _mapper.Map<UserAccount>(register);

        userAccount.Id = await _usersClient.SendCreateRequestAsync(
            userCreate: _mapper.Map<UserCreate>(register),
            jwt: _jwtService.GenerateJwt(new Account<Guid>
            {
                Permissions = new[] { new Permission { Id = HttpClientPermissions.UsersCreate } }
            }));

        userAccount.PasswordHash = BCryptNet.HashPassword(register.Password);
        await _accountsRepository.InsertAsync(userAccount);

        return _jwtService.GenerateJwt(_mapper.Map<Account<Guid>>(userAccount));
    }

    public async Task<string> LoginAsync(UserAccountLogin login)
    {
        UserAccount userAccount = await _accountsRepository.GetByEmailAsync(login.Email)
                                  ?? throw new BadRequestException(
                                      $"User with email {login.Email} dose not exist");

        if (!BCryptNet.Verify(text: login.Password, hash: userAccount.PasswordHash))
            throw new BadRequestException("Incorrect password");

        return _jwtService.GenerateJwt(_mapper.Map<Account<Guid>>(userAccount));
    }

    public async Task ChangePassword(UserAccountChangePassword changePassword, Account<Guid> account)
    {
        UserAccount userAccount = (await _accountsRepository.GetByIdAsync(account.Id))!;

        if (!BCryptNet.Verify(text: changePassword.CurrentPassword, hash: userAccount.PasswordHash))
            throw new BadRequestException("Incorrect password");

        if (changePassword.CurrentPassword == changePassword.NewPassword)
            throw new BadRequestException("This password already use");

        userAccount.PasswordHash = BCryptNet.HashPassword(changePassword.NewPassword);
        await _accountsRepository.UpdateAsync(userAccount);
    }

    public async Task DeleteAccount(Account<Guid> account)
    {
        await _usersClient.SendUserRequestAsync(
            userId: account.Id,
            jwt: _jwtService.GenerateJwt(new Account<Guid>
            {
                Permissions = new[] { new Permission { Id = HttpClientPermissions.UsersDelete } }
            }));

        await _accountsRepository.DeleteAsync(account.Id);
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