using AutoMapper;
using HelpDeskSystem.Constants;
using HelpDeskSystem.Exceptions;
using HelpDeskSystem.Models.Roles;
using HelpDeskSystem.Models.User;
using HelpDeskSystem.Repositories;
using BCryptNet = BCrypt.Net.BCrypt;

namespace HelpDeskSystem.Services;

public class AccountsService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public AccountsService(IApplicationContext applicationContext, IJwtService jwtService, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<string> RegisterAsync(UserCreate userCreate)
    {
        if (await _applicationContext.Users.GetByEmailAsync(userCreate.Email) != null)
            throw new BadRequestException("User with this email already exist");

        var user = _mapper.Map<User>(userCreate);
        user.Id = Guid.NewGuid();
        user.PasswordHash = BCryptNet.HashPassword(userCreate.Password);
        await _applicationContext.Users.InsertAsync(user);
        await _applicationContext.Roles.AddToUser(RoleNames.User, user.Id);

        return _jwtService.GenerateJwt(user);
    }

    public async Task<string> LoginAsync(UserLogin userLogin)
    {
        User? user = await _applicationContext.Users.GetByEmailAsync(userLogin.Email);
        if (user == null)
            throw new BadRequestException("User with this email does not exist");

        if (!BCryptNet.Verify(userLogin.Password, user.PasswordHash))
            throw new BadRequestException("Incorrect password");

        IEnumerable<Role> userRoles = await _applicationContext.Roles.GetByUserIdAsync(user.Id);
        return _jwtService.GenerateJwt(user, userRoles);
    }
}