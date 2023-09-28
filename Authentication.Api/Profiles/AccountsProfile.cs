using Authentication.Api.Models.Accounts;
using Authentication.Api.Models.Http.Users;
using Infrastructure.Authentication.Models;
using AutoMapper;

namespace Authentication.Api.Profiles;

public class AccountsProfile : Profile
{
    public AccountsProfile()
    {
        CreateMap<UserAccount, UserAccountView>();
        CreateMap<UserAccountRegister, UserAccount>();
        CreateMap<UserAccountRegister, UserCreate>();
        CreateMap<UserAccount, Account<Guid>>()
            .ForMember(
                destinationMember: _ => _.Permissions,
                memberOptions: _ => _.MapFrom(account =>
                    account.Roles
                        .SelectMany(role => role.Permissions)
                        .GroupBy(permission => permission.Id)
                        .Select(group => group.First())
                        .ToList()));
    }
}