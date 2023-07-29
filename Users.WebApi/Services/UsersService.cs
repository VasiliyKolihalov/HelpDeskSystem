using Authentication.Infrastructure.Models;
using AutoMapper;
using Infrastructure.Exceptions;
using Infrastructure.Services.Messaging;
using Users.WebApi.Models.Users;
using Users.WebApi.Repositories;
using static Users.WebApi.Constants.PermissionNames.UserPermissions;

namespace Users.WebApi.Services;

public class UsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly IMapper _mapper;

    public UsersService(IUsersRepository usersRepository, IRabbitMqPublisher rabbitMqPublisher, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _rabbitMqPublisher = rabbitMqPublisher;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserView>> GetAllAsync()
    {
        IEnumerable<User> users = await _usersRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<UserView>>(users);
    }

    public async Task<UserView> GetByIdAsync(Guid id)
    {
        User user = await _usersRepository.GetByIdAsync(id)
                    ?? throw new NotFoundException($"User with id: {id} not found");

        return _mapper.Map<UserView>(user);
    }

    public async Task<Guid> CreateAsync(UserCreate userCreate)
    {
        var user = _mapper.Map<User>(userCreate);
        user.Id = Guid.NewGuid();

        await _usersRepository.InsertAsync(user);
        _rabbitMqPublisher.PublishMessage(user, "users.created");
        return user.Id;
    }

    public async Task UpdateAsync(UserUpdate userUpdate, Account<Guid> account)
    {
        if (!await _usersRepository.IsExistsAsync(userUpdate.Id))
            throw new NotFoundException($"User with id: {userUpdate.Id} not found");

        if (userUpdate.Id != account.Id || !account.HasPermission(Update))
            throw new UnauthorizedException();

        var user = _mapper.Map<User>(userUpdate);

        await _usersRepository.UpdateAsync(user);
        _rabbitMqPublisher.PublishMessage(user, "users.updated");
    }

    public async Task DeleteAsync(Guid userId)
    {
        User user = await _usersRepository.GetByIdAsync(userId)
                    ?? throw new NotFoundException($"User with id: {userId} not found");

        await _usersRepository.DeleteAsync(userId);
        _rabbitMqPublisher.PublishMessage(user, "users.deleted");
    }
}