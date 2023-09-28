using Authentication.Api.Models.Roles;

namespace Authentication.Api.Models.Accounts;

public class UserAccountView
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsEmailConfirm { get; set; }
    public IEnumerable<UserRoleView> Roles { get; set; }
}