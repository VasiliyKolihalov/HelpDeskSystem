using HelpDeskSystem.Models.Roles;

namespace HelpDeskSystem.Models.User;

public class UserView
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public IEnumerable<RolePreview> Roles { get; set; }
}