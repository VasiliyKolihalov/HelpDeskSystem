using HelpDeskSystem.Models.Roles;
using HelpDeskSystem.Models.User;

namespace HelpDeskSystem.Services;

public interface IJwtService
{
    public string GenerateJwt(User user, IEnumerable<Role>? userRoles = null);
}