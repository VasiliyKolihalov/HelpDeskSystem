using AutoMapper;
using Infrastructure.Exceptions;
using Infrastructure.Services;
using Infrastructure.Services.Messaging;
using Users.WebApi.Models.Users;
using Users.WebApi.Repositories;

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

    public async Task<UserView> CreateAsync(UserCreate userCreate)
    {
        var user = _mapper.Map<User>(userCreate);
        user.Id = Guid.NewGuid();

        await _usersRepository.InsertAsync(user);
        _rabbitMqPublisher.PublishMessage(user, "users.created");
        return _mapper.Map<UserView>(user);
    }

    public async Task<UserView> UpdateAsync(UserUpdate userUpdate)
    {
        _ = await _usersRepository.GetByIdAsync(userUpdate.Id)
            ?? throw new NotFoundException($"User with id: {userUpdate.Id} not found");

        var user = _mapper.Map<User>(userUpdate);

        await _usersRepository.UpdateAsync(user);
        _rabbitMqPublisher.PublishMessage(user, "users.updated");
        return _mapper.Map<UserView>(user);
    }

    public async Task<UserView> DeleteAsync(Guid id)
    {
        User user = await _usersRepository.GetByIdAsync(id)
                    ?? throw new NotFoundException($"User with id: {id} not found");

        await _usersRepository.DeleteAsync(id);
        _rabbitMqPublisher.PublishMessage(user, "users.deleted");
        return _mapper.Map<UserView>(user);
    }
}