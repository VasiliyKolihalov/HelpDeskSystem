using Authentication.Infrastructure.Models;
using AutoFixture.NUnit3;
using AutoMapper;
using FluentAssertions;
using Infrastructure.Exceptions;
using Moq;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services;
using SupportTickets.WebApi.Services.Clients;
using static Moq.Times;
using Message = SupportTickets.WebApi.Models.Messages.Message;

namespace SupportTickets.Tests;

public class Tests
{
    private Mock<ISupportTicketsRepository> _supportTicketsRepository = null!;
    private Mock<IUsersRepository> _usersRepository = null!;
    private Mock<IAccountsClient> _accountsClient = null!;
    private Mock<IMapper> _mapper = null!;
    private SupportTicketsService _supportTicketsService = null!;

    [SetUp]
    public void Setup()
    {
        _supportTicketsRepository = new Mock<ISupportTicketsRepository>();
        _usersRepository = new Mock<IUsersRepository>();
        _accountsClient = new Mock<IAccountsClient>();
        _mapper = new Mock<IMapper>();

        _supportTicketsService = new SupportTicketsService(
            _supportTicketsRepository.Object,
            _usersRepository.Object,
            _accountsClient.Object,
            _mapper.Object);
    }


    [Test, AutoData]
    public async Task GetAllAsync_CorrectCase_Pass(IEnumerable<SupportTicket> supportTickets)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetAllAsync()).ReturnsAsync(supportTickets);

        // Act
        IEnumerable<SupportTicketPreview> result = await _supportTicketsService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        _supportTicketsRepository.Verify(_ => _.GetAllAsync(), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetBasedOnAccountIdAsync_CorrectCase_Pass(
        IEnumerable<SupportTicket> supportTickets,
        Guid accountId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetBasedOnAccountAsync(accountId)).ReturnsAsync(supportTickets);

        // Act
        IEnumerable<SupportTicketPreview> result = await _supportTicketsService.GetBasedOnAccountIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        _supportTicketsRepository.Verify(_ => _.GetBasedOnAccountAsync(accountId), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetByIdAsync_CorrectCase_Pass(SupportTicket supportTicket, Guid accountId)
    {
        // Arrange
        supportTicket.User = new User { Id = accountId };
        var account = new Account<Guid> { Id = accountId };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);

        // Act
        await _supportTicketsService.GetByIdAsync(supportTicket.Id, account);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<SupportTicketView>(supportTicket), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetByIdAsync_NotExistsSupportTicket_Throw(Guid supportTicketId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService
            .GetByIdAsync(supportTicketId, It.IsAny<Account<Guid>>());

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task GetByIdAsync_AccountIsNotRelated_Throw(SupportTicket supportTicket, Account<Guid> account)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.GetByIdAsync(supportTicket.Id, account);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task CreateAsync_CorrectCase_Pass(SupportTicket supportTicket, Account<Guid> account, User user)
    {
        // Arrange
        var supportTicketCreate = It.IsNotNull<SupportTicketCreate>();
        _mapper.Setup(_ => _.Map<SupportTicket>(supportTicketCreate)).Returns(supportTicket);
        _mapper.Setup(_ => _.Map<User>(account)).Returns(user);

        // Act
        Guid result = await _supportTicketsService.CreateAsync(supportTicketCreate, account);

        // Assert
        result.Should().NotBeEmpty();
        _mapper.Verify(_ => _.Map<SupportTicket>(supportTicketCreate), Once);
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.VerifyNoOtherCalls();
        _supportTicketsRepository.Verify(
            _ => _.InsertAsync(It.Is<SupportTicket>(ticket => ticket.Id == result && ticket.User.Id == user.Id)),
            Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdateAsync_CorrectCase_Pass(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account,
        SupportTicket supportTicket)
    {
        // Arrange
        supportTicket.User = new User { Id = account.Id };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);
        var supportTicketUpdated = It.IsNotNull<SupportTicket>();
        _mapper.Setup(_ => _.Map<SupportTicket>(supportTicketUpdate)).Returns(supportTicketUpdated);

        // Act
        await _supportTicketsService.UpdateAsync(supportTicketUpdate, account);

        // Result
        _mapper.Verify(_ => _.Map<SupportTicket>(supportTicketUpdate));
        _mapper.VerifyNoOtherCalls();
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketUpdate.Id), Once);
        _supportTicketsRepository.Verify(_ => _.UpdateAsync(supportTicketUpdated), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdateAsync_SupportTicketNotExists_Throw(SupportTicketUpdate supportTicketUpdate)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService
            .UpdateAsync(supportTicketUpdate, It.IsAny<Account<Guid>>());

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketUpdate.Id} not found");
    }

    [Test, AutoData]
    public async Task UpdateAsync_AccountIsNotRelated_Throw(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account,
        SupportTicket supportTicket)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateAsync(supportTicketUpdate, account);

        // Result
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task DeleteAsync_CorrectCase_Pass(Guid supportTicketId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.IsExistsAsync(supportTicketId)).ReturnsAsync(true);

        // Act
        await _supportTicketsService.DeleteAsync(supportTicketId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.IsExistsAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(_ => _.DeleteAsync(supportTicketId), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task DeleteAsync_SupportTicketNotExists_Throw(Guid supportTicketId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteAsync(supportTicketId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task SetAgentAsync_CorrectCase_Pass(SupportTicket supportTicket, Guid userId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        _usersRepository.Setup(_ => _.IsExistsAsync(userId)).ReturnsAsync(true);
        var account = new Account<Guid> { Roles = new[] { new Role { Id = "agent" } } };
        _accountsClient.Setup(_ => _.SendGetRequestAsync(userId)).ReturnsAsync(account);

        // Act
        await _supportTicketsService.SetAgentAsync(supportTicket.Id, userId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
        _supportTicketsRepository.Verify(
            _ => _.UpdateAsync(It.Is<SupportTicket>(ticket => ticket == supportTicket && ticket.Agent!.Id == userId)),
            Once);
        _usersRepository.Verify(_ => _.IsExistsAsync(userId), Once);
        _accountsClient.Verify(_ => _.SendGetRequestAsync(userId), Once);
    }

    [Test, AutoData]
    public async Task SetAgentAsync_SupportTicketNotExists_Throw(Guid supportTicketId, Guid userId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.SetAgentAsync(supportTicketId, userId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task SetAgentAsync_UserNotExists_Throw(SupportTicket supportTicket, Guid userId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id))
            .ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.SetAgentAsync(supportTicket.Id, userId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"User with id: {userId} not found");
    }

    [Test, AutoData]
    public async Task SetAgentAsync_AccountHaveNoRole_Throw(SupportTicket supportTicket, Guid userId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id))
            .ReturnsAsync(supportTicket);
        _usersRepository.Setup(_ => _.IsExistsAsync(userId)).ReturnsAsync(true);
        var account = new Account<Guid> { Roles = Array.Empty<Role>() };
        _accountsClient.Setup(_ => _.SendGetRequestAsync(userId)).ReturnsAsync(account);

        // Act
        Func<Task> action = async () => await _supportTicketsService.SetAgentAsync(supportTicket.Id, userId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("User are not agent");
    }

    [Test, AutoData]
    public async Task AddMessageAsync_CorrectCase_Pass(MessageCreate messageCreate, SupportTicket supportTicket,
        Account<Guid> account)
    {
        // Arrange
        var user = new User { Id = account.Id };
        supportTicket.User = user;
        var messageFromMapper = new Message();
        _mapper.Setup(_ => _.Map<Message>(messageCreate)).Returns(messageFromMapper);
        _mapper.Setup(_ => _.Map<User>(account)).Returns(user);
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Guid result = await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        result.Should().NotBeEmpty();
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(messageCreate.SupportTicketId), Once);
        _supportTicketsRepository.Verify(_ => _.AddMessageAsync(It.Is<Message>(message => message == messageFromMapper
            && message.User.Id == account.Id)), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<Message>(messageCreate), Once);
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AddMessageAsync_SupportTicketNotExists_Throw(
        MessageCreate messageCreate,
        SupportTicket supportTicket,
        Account<Guid> account)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {messageCreate.SupportTicketId} not found");
    }

    [Test, AutoData]
    public async Task AddMessageAsync_AccountIsNotRelated_Pass(
        MessageCreate messageCreate,
        SupportTicket supportTicket,
        Account<Guid> account)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task UpdateMessageAsync_CorrectCase_Pass(MessageUpdate messageUpdate, Guid accountId, Message message)
    {
        // Arrange
        message.User = new User { Id = accountId };
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageUpdate.Id))
            .ReturnsAsync(message);
        var updatedMessage = new Message();
        _mapper.Setup(_ => _.Map<Message>(messageUpdate)).Returns(updatedMessage);

        // Act
        await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetMessageByIdAsync(messageUpdate.Id), Once);
        _supportTicketsRepository.Verify(_ => _.UpdateMessageAsync(updatedMessage), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<Message>(messageUpdate), Once);
        _mapper.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdateMessageAsync_SupportTicketNotExists_Throw(MessageUpdate messageUpdate, Guid accountId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Message with id: {messageUpdate.Id} not found");
    }
    
    [Test, AutoData]
    public async Task UpdateMessageAsync_AccountIsNotRelated_Throw(MessageUpdate messageUpdate, Guid accountId, Message message)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageUpdate.Id))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }
    
    [Test, AutoData]
    public async Task DeleteMessageAsync_CorrectCase_Pass(Guid messageId, Guid accountId, Message message)
    {
        // Arrange
        message.User = new User { Id = accountId };
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetMessageByIdAsync(messageId), Once);
        _supportTicketsRepository.Verify(_ => _.DeleteMessageAsync(messageId), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task DeleteMessageAsync_SupportTicketNotExists_Throw(Guid messageId, Guid accountId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Message with id: {messageId} not found");
    }
    
    [Test, AutoData]
    public async Task DeleteMessageAsync_AccountIsNotRelated_Throw(Guid messageId, Guid accountId, Message message)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }
}