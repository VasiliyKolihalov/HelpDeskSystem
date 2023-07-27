using Authentication.Infrastructure.Models;
using Authentication.WebApi.Models.Accounts;
using Authentication.WebApi.Models.Http.Users;
using AutoMapper;

namespace Authentication.WebApi.Profiles;

public class AccountsProfile : Profile
{
    public AccountsProfile()
    {
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