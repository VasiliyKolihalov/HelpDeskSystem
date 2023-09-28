using Authentication.Api.Models.Permissions;
using Authentication.Api.Repositories.Permissions;
using AutoMapper;

namespace Authentication.Api.Services;

public class PermissionsService
{
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IMapper _mapper;

    public PermissionsService(IPermissionsRepository permissionsRepository, IMapper mapper)
    {
        _permissionsRepository = permissionsRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserPermissionView>> GetAllAsync()
    {
        return _mapper.Map<IEnumerable<UserPermissionView>>(await _permissionsRepository.GetAllAsync());
    }
}