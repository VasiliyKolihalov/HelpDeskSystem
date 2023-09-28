using Authentication.Api.Models.Roles;
using Authentication.Api.Repositories.Permissions;
using Authentication.Api.Repositories.Roles;
using AutoMapper;
using Infrastructure.Exceptions;

namespace Authentication.Api.Services;

public class RolesService
{
    private readonly IRolesRepository _rolesRepository;
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IMapper _mapper;

    public RolesService(
        IRolesRepository rolesRepository,
        IPermissionsRepository permissionsRepository,
        IMapper mapper)
    {
        _rolesRepository = rolesRepository;
        _permissionsRepository = permissionsRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserRoleView>> GetAllAsync()
    {
        IEnumerable<UserRole> roles = await _rolesRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserRoleView>>(roles);
    }

    public async Task<UserRoleView> GetByIdAsync(string id)
    {
        UserRole role = await _rolesRepository.GetByIdAsync(id)
                        ?? throw new NotFoundException($"Role with id {id} not found");

        return _mapper.Map<UserRoleView>(role);
    }

    public async Task CreateAsync(UserRoleCreate userRoleCreate)
    {
        if (await _rolesRepository.IsExistsAsync(userRoleCreate.Id))
            throw new NotFoundException($"Role with id {userRoleCreate.Id} already exists");

        foreach (string permissionId in userRoleCreate.PermissionIds)
        {
            if (!await _permissionsRepository.IsExistsAsync(permissionId))
                throw new NotFoundException($"Permission with id {permissionId} not exist");
        }

        var role = _mapper.Map<UserRole>(userRoleCreate);
        await _rolesRepository.InsertAsync(role);
        foreach (string permissionId in userRoleCreate.PermissionIds)
        {
            await _rolesRepository.AddPermissionAsync(userRoleCreate.Id, permissionId);
        }
    }

    public async Task UpdateAsync(UserRoleUpdate userRoleUpdate)
    {
        _ = await _rolesRepository.GetByIdAsync(userRoleUpdate.Id)
            ?? throw new NotFoundException($"Role with id {userRoleUpdate.Id} not found");

        foreach (string permissionId in userRoleUpdate.PermissionIds)
        {
            if (!await _permissionsRepository.IsExistsAsync(permissionId))
                throw new NotFoundException($"Permission with id {permissionId} not exist");
        }

        await _rolesRepository.RemoveAllPermissionsAsync(userRoleUpdate.Id);
        foreach (string permissionId in userRoleUpdate.PermissionIds)
        {
            await _rolesRepository.AddPermissionAsync(userRoleUpdate.Id, permissionId);
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (!await _rolesRepository.IsExistsAsync(id))
            throw new NotFoundException($"Role with id {id} not found");

        await _rolesRepository.DeleteAsync(id);
    }
}