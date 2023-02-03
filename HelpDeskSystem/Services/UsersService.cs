using AutoMapper;
using HelpDeskSystem.Exceptions;
using HelpDeskSystem.Models.Roles;
using HelpDeskSystem.Models.User;
using HelpDeskSystem.Repositories;
using static HelpDeskSystem.Constants.PermissionNames;

namespace HelpDeskSystem.Services;

public class UsersService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public UsersService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserPreview>> GetAllAsync()
    {
        IEnumerable<User> users = await _applicationContext.Users.GetAllAsync();

        return _mapper.Map<IEnumerable<UserPreview>>(users);
    }

    public async Task<UserView> GetByIdAsync(Guid id)
    {
        User user = await _applicationContext.Users.GetByIdAsync(id);
        var userView = _mapper.Map<UserView>(user);
        IEnumerable<Role> userRoles = await _applicationContext.Roles.GetByUserIdAsync(id);
        userView.Roles = _mapper.Map<IEnumerable<RolePreview>>(userRoles);

        return userView;
    }

    public async Task<UserPreview> UpdateAsync(UserUpdate userUpdate, Guid requestUserId)
    {
        await _applicationContext.Users.GetByIdAsync(userUpdate.Id);

        IEnumerable<Role> requestUserRoles = await _applicationContext.Roles.GetByUserIdAsync(requestUserId);
        IEnumerable<string> requestUserPermissions = requestUserRoles.SelectMany(_ => _.Permissions);

        if (userUpdate.Id != requestUserId && !requestUserPermissions.Contains(UsersPermissions.Update))
            throw new ForbiddenException("User does not have enough rights for this action");

        var user = _mapper.Map<User>(userUpdate);

        await _applicationContext.Users.UpdateAsync(user);

        User updatedUser = await _applicationContext.Users.GetByIdAsync(userUpdate.Id);
        return _mapper.Map<UserPreview>(updatedUser);
    }

    public async Task<UserPreview> DeleteAsync(Guid userToDeleteId, Guid requestUserId)
    {
        User user = await _applicationContext.Users.GetByIdAsync(userToDeleteId);

        IEnumerable<Role> requestUserRoles = await _applicationContext.Roles.GetByUserIdAsync(requestUserId);
        IEnumerable<string> requestUserPermissions = requestUserRoles.SelectMany(_ => _.Permissions);
        
        if (userToDeleteId != requestUserId && !requestUserPermissions.Contains(UsersPermissions.Delete))
            throw new ForbiddenException("User does not have enough rights for this action");

        await _applicationContext.Users.DeleteAsync(userToDeleteId);

        return _mapper.Map<UserPreview>(user);
    }
}