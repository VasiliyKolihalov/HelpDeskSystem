using Authentication.WebApi.Models.Roles;

namespace Authentication.WebApi.Models.Accounts;

public class UserAccount
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsEmailConfirm { get; set; }
    public string PasswordHash { get; set; }
    public IEnumerable<UserRole> Roles { get; set; }
}