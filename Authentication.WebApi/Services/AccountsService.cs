﻿using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using Authentication.WebApi.Models.Accounts;
using Authentication.WebApi.Models.Http.Users;
using Authentication.WebApi.Models.Messaging;
using Authentication.WebApi.Repositories.Accounts;
using Authentication.WebApi.Repositories.ConfirmCodes;
using Authentication.WebApi.Repositories.Roles;
using Authentication.WebApi.Services.Clients.Users;
using AutoMapper;
using Infrastructure.Exceptions;
using Infrastructure.Services.Messaging;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Authentication.WebApi.Services;

public class AccountsService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IConfirmCodesRepository _confirmCodesRepository;
    private readonly IJwtService _jwtService;
    private readonly IUsersClient _usersClient;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly IEmailConfirmCodeProvider _emailConfirmCodeProvider;
    private readonly IMapper _mapper;

    public AccountsService(
        IAccountsRepository accountsRepository,
        IRolesRepository rolesRepository,
        IConfirmCodesRepository confirmCodesRepository,
        IJwtService jwtService,
        IUsersClient usersClient,
        IRabbitMqPublisher rabbitMqPublisher,
        IEmailConfirmCodeProvider emailConfirmCodeProvider,
        IMapper mapper)
    {
        _accountsRepository = accountsRepository;
        _rolesRepository = rolesRepository;
        _confirmCodesRepository = confirmCodesRepository;
        _jwtService = jwtService;
        _usersClient = usersClient;
        _rabbitMqPublisher = rabbitMqPublisher;
        _emailConfirmCodeProvider = emailConfirmCodeProvider;
        _mapper = mapper;
    }

    public async Task<string> RegisterAsync(UserAccountRegister register)
    {
        if (await _accountsRepository.IsExistsAsync(register.Email))
            throw new BadRequestException($"Account with email {register.Email} already exist");

        var userAccount = _mapper.Map<UserAccount>(register);

        userAccount.Id = await _usersClient.SendPostRequestAsync(_mapper.Map<UserCreate>(register));

        userAccount.PasswordHash = BCryptNet.HashPassword(register.Password);
        userAccount.IsEmailConfirm = false;
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

    public async Task SendEmailConfirmCodeAsync(Guid accountId)
    {
        UserAccount account = (await _accountsRepository.GetByIdAsync(accountId))!;

        if (account.IsEmailConfirm)
            throw new BadRequestException("Email already confirm");

        if (await _confirmCodesRepository.IsExistsAsync(accountId))
            await _confirmCodesRepository.DeleteAsync(accountId);

        string code = _emailConfirmCodeProvider.Generate();
        await _confirmCodesRepository.InsertAsync(accountId, code);

        UserView userView = await _usersClient.SendGetRequestAsync(userId: accountId);

        _rabbitMqPublisher.PublishMessage(
            message: new RequestEmailConfirm
            {
                ConfirmCode = code,
                Email = account.Email,
                FirstName = userView.FirstName
            },
            routingKey: "notifications.requested_email_confirm");
    }

    public async Task ConfirmEmailAsync(string confirmCode, Guid accountId)
    {
        UserAccount account = (await _accountsRepository.GetByIdAsync(accountId))!;

        if (account.IsEmailConfirm)
            throw new BadRequestException("Email already confirm");

        string trueConfirmCode = await _confirmCodesRepository.GetByAccountIdAsync(accountId)
                                 ?? throw new BadRequestException("Account didn't request for confirm code");

        if (confirmCode != trueConfirmCode)
            throw new BadRequestException("Incorrect confirm code");

        account.IsEmailConfirm = true;
        await _accountsRepository.UpdateAsync(account);
        await _confirmCodesRepository.DeleteAsync(account.Id);
    }

    public async Task ChangeEmailAsync(ChangeEmail changeEmail, Guid accountId)
    {
        UserAccount account = (await _accountsRepository.GetByIdAsync(accountId))!;

        if (account.Email == changeEmail.NewEmail)
            throw new BadRequestException("New email must not be the same as the old");

        account.Email = changeEmail.NewEmail;
        account.IsEmailConfirm = false;
        await _accountsRepository.UpdateAsync(account);
    }

    public async Task ChangePasswordAsync(ChangePassword changePassword, Guid accountId)
    {
        UserAccount userAccount = (await _accountsRepository.GetByIdAsync(accountId))!;

        if (!BCryptNet.Verify(text: changePassword.CurrentPassword, hash: userAccount.PasswordHash))
            throw new BadRequestException("Incorrect password");

        if (changePassword.CurrentPassword == changePassword.NewPassword)
            throw new BadRequestException("This password already use");

        userAccount.PasswordHash = BCryptNet.HashPassword(changePassword.NewPassword);
        await _accountsRepository.UpdateAsync(userAccount);
    }

    public async Task DeleteAsync(Guid accountId)
    {
        await _usersClient.SendDeleteRequestAsync(accountId);
        await _accountsRepository.DeleteAsync(accountId);
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