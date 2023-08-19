using Authentication.Infrastructure.Models;
using AutoMapper;
using Dapper;
using Infrastructure.Exceptions;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Services;
using SupportTickets.WebApi.Services.JobsManagers;
using Message = SupportTickets.WebApi.Models.Messages.Message;

namespace SupportTickets.Tests;

public class Tests
{
    private Mock<ISupportTicketsRepository> _supportTicketsRepository = null!;
    private Mock<IEscalationsManager> _escalationsManager = null!;
    private Mock<IMapper> _mapper = null!;
    private SupportTicketsService _supportTicketsService = null!;

    [SetUp]
    public void Setup()
    {
        _supportTicketsRepository = new Mock<ISupportTicketsRepository>();
        _escalationsManager = new Mock<IEscalationsManager>();
        _mapper = new Mock<IMapper>();

        _supportTicketsService = new SupportTicketsService(
            _supportTicketsRepository.Object,
            _escalationsManager.Object,
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
    public async Task GetFreeAsync_CorrectCase_Pass(List<SupportTicket> supportTickets, Guid accountId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetAllWithoutAgent()).ReturnsAsync(supportTickets);

        Guid supportTicketIdWhereAccountWasNotAgent = supportTickets.First().Id;
        _supportTicketsRepository
            .Setup(_ => _.GetAgentsHistoryAsync(supportTicketIdWhereAccountWasNotAgent))
            .ReturnsAsync(Array.Empty<User>());

        var formerAgents = new[] { new User { Id = accountId } };
        _supportTicketsRepository
            .Setup(repository => repository.GetAgentsHistoryAsync(It.IsNotIn(supportTicketIdWhereAccountWasNotAgent)))
            .ReturnsAsync(formerAgents);

        // Act
        List<SupportTicketPreview> result = (await _supportTicketsService.GetFreeAsync(accountId)).AsList();

        // Assert
        result.Should().NotBeNull();
        _supportTicketsRepository.Verify(_ => _.GetAllWithoutAgent(), Once);
        _supportTicketsRepository.Verify(_ => _.GetAgentsHistoryAsync(It.IsAny<Guid>()), Exactly(supportTickets.Count));
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(
            expression: mapper => mapper.Map<IEnumerable<SupportTicketPreview>>(
                It.Is<IEnumerable<SupportTicket>>(_ =>
                    _.Count() == 1 &&
                    _.First().Id == supportTicketIdWhereAccountWasNotAgent)),
            times: Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetByAccountIdAsync_CorrectCase_Pass(
        IEnumerable<SupportTicket> supportTickets,
        Guid accountId)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetBasedOnAccountAsync(accountId)).ReturnsAsync(supportTickets);

        // Act
        IEnumerable<SupportTicketPreview> result = await _supportTicketsService.GetByAccountIdAsync(accountId);

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
    public async Task CreateAsync_CorrectCase_Pass(SupportTicketCreate supportTicketCreate, Account<Guid> account)
    {
        // Arrange
        var supportTicketFromMapper = new SupportTicket();
        _mapper.Setup(_ => _.Map<SupportTicket>(supportTicketCreate)).Returns(supportTicketFromMapper);
        var userFromMapper = new User { Id = Guid.NewGuid() };
        _mapper.Setup(_ => _.Map<User>(account)).Returns(userFromMapper);

        // Act
        Guid result = await _supportTicketsService.CreateAsync(supportTicketCreate, account);

        // Assert
        result.Should().NotBeEmpty();
        _mapper.Verify(_ => _.Map<SupportTicket>(supportTicketCreate), Once);
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.VerifyNoOtherCalls();
        _supportTicketsRepository.Verify(
            expression: repository =>
                repository.InsertAsync(It.Is<SupportTicket>(supportTicket =>
                    supportTicket == supportTicketFromMapper &&
                    supportTicket.User.Id == userFromMapper.Id &&
                    supportTicket.Status == SupportTicketStatus.Open)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdateAsync_CorrectCase_Pass(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User { Id = account.Id },
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);
        var updatedSupportTicket = new SupportTicket();
        _mapper.Setup(_ => _.Map<SupportTicket>(supportTicketUpdate)).Returns(updatedSupportTicket);

        // Act
        await _supportTicketsService.UpdateAsync(supportTicketUpdate, account);

        // Result
        _mapper.Verify(_ => _.Map<SupportTicket>(supportTicketUpdate));
        _mapper.VerifyNoOtherCalls();
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketUpdate.Id), Once);
        _supportTicketsRepository.Verify(_ => _.UpdateAsync(updatedSupportTicket), Once);
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
    public async Task UpdateAsync_SupportTicketNotOpen_Throw(SupportTicketUpdate supportTicketUpdate)
    {
        // Arrange
        var supportTicket = new SupportTicket { Status = SupportTicketStatus.Close };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService
            .UpdateAsync(supportTicketUpdate, It.IsAny<Account<Guid>>());

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicketUpdate.Id} not open");
    }

    [Test, AutoData]
    public async Task UpdateAsync_AccountIsNotRelated_Throw(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User(),
            Status = SupportTicketStatus.Open
        };
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
    public async Task CloseAsync_CorrectCase_Pass(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            Agent = new User { Id = account.Id }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        await _supportTicketsService.CloseAsync(supportTicketId, account);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: repository =>
                repository.UpdateAsync(It.Is<SupportTicket>(supportTicket =>
                    supportTicket == supportTicketFromRepository &&
                    supportTicket.Status == SupportTicketStatus.Close)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _escalationsManager.Verify(_ => _.CancelEscalationFor(supportTicketId));
        _escalationsManager.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task CloseAsync_AccountIsNotRelated_Throw(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.CloseAsync(supportTicketId, account);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task CloseAsync_SupportTicketNotOpen_Throw(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Close,
            Agent = new User { Id = account.Id }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.CloseAsync(supportTicketId, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not open");
    }

    [Test, AutoData]
    public async Task AppointAgentAsync_CorrectCase_Pass(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket();
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId))
            .ReturnsAsync(supportTicketFromRepository);
        _supportTicketsRepository
            .Setup(_ => _.GetAgentsHistoryAsync(supportTicketId))
            .ReturnsAsync(Array.Empty<User>());
        var user = new User();
        _mapper.Setup(_ => _.Map<User>(account)).Returns(user);

        // Act
        await _supportTicketsService.AppointAgentAsync(supportTicketId, account);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(_ => _.GetAgentsHistoryAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: repository =>
                repository.UpdateAsync(It.Is<SupportTicket>(supportTicket =>
                    supportTicket == supportTicketFromRepository &&
                    supportTicket.Priority == SupportTicketPriority.Low &&
                    supportTicket.Agent == user)),
            times: Once);
        _supportTicketsRepository.Verify(_ => _.AddToAgentsHistoryAsync(supportTicketId, account.Id), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _escalationsManager.Verify(_ => _.AssignEscalationFor(supportTicketId, TimeSpan.FromDays(7)), Once);
        _escalationsManager.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AppointAgentAsync_SupportTicketNotExists_Throw(Guid supportTicketId, Account<Guid> account)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.AppointAgentAsync(supportTicketId, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task AppointAgentAsync_SupportTicketNotOpen_Throw(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Close
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AppointAgentAsync(supportTicketId, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not open");
    }

    [Test, AutoData]
    public async Task AppointAgentAsync_SupportTicketAlreadyHaveAgent_Throw(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            Agent = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId))
            .ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AppointAgentAsync(supportTicketId, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} already have agent");
    }

    [Test, AutoData]
    public async Task AppointAgentAsync_AccountAlreadyWasAgentForSupportTicket_Throw(
        Guid supportTicketId,
        Account<Guid> account)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket();
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId))
            .ReturnsAsync(supportTicketFromRepository);
        var formerAgents = new[] { new User { Id = account.Id } };
        _supportTicketsRepository
            .Setup(_ => _.GetAgentsHistoryAsync(supportTicketId))
            .ReturnsAsync(formerAgents);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AppointAgentAsync(supportTicketId, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                $"Account with id: {account.Id} was already agent for SupportTicket with id: {supportTicketId}");
    }

    [Test, AutoData]
    public async Task AddMessageAsync_CorrectCase_Pass(MessageCreate messageCreate, Account<Guid> account)
    {
        // Arrange
        var messageFromMapper = new Message();
        _mapper.Setup(_ => _.Map<Message>(messageCreate)).Returns(messageFromMapper);
        var user = new User { Id = account.Id };
        _mapper.Setup(_ => _.Map<User>(account)).Returns(user);
        var supportTicket = new SupportTicket
        {
            User = user,
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Guid result = await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        result.Should().NotBeEmpty();
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(messageCreate.SupportTicketId), Once);
        _supportTicketsRepository.Verify(repository =>
                repository.AddMessageAsync(It.Is<Message>(message =>
                    message == messageFromMapper &&
                    message.User.Id == account.Id)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<Message>(messageCreate), Once);
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AddMessageAsync_SupportTicketNotExists_Throw(
        MessageCreate messageCreate,
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
    public async Task AddMessageAsync_SupportTicketNotOpen_Throw(
        MessageCreate messageCreate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Close
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {messageCreate.SupportTicketId} not open");
    }

    [Test, AutoData]
    public async Task AddMessageAsync_AccountIsNotRelated_Throw(
        MessageCreate messageCreate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User(),
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task UpdateMessageAsync_CorrectCase_Pass(MessageUpdate messageUpdate, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageUpdate.Id)).ReturnsAsync(message);
        var updatedMessage = new Message();
        _mapper.Setup(_ => _.Map<Message>(messageUpdate)).Returns(updatedMessage);


        // Act
        await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
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
    public async Task UpdateMessageAsync_SupportTicketNotOpen_Throw(MessageUpdate messageUpdate, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Close
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageUpdate.Id))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
    }

    [Test, AutoData]
    public async Task UpdateMessageAsync_AccountIsNotRelated_Throw(MessageUpdate messageUpdate, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User()
        };
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
    public async Task DeleteMessageAsync_CorrectCase_Pass(Guid messageId, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
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
    public async Task DeleteMessageAsync_SupportTicketNotOpen_Throw(Guid messageId, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Close
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
    }

    [Test, AutoData]
    public async Task DeleteMessageAsync_AccountIsNotRelated_Throw(Guid messageId, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_CorrectCase_Pass(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Arrange
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
        var supportTicket = new SupportTicket
        {
            Agent = new User
            {
                Id = accountId
            },
            Status = SupportTicketStatus.Open,
            Solutions = Array.Empty<Solution>()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(message.SupportTicketId)).ReturnsAsync(supportTicket);
        var solutionFromMapper = new Solution();
        _mapper.Setup(_ => _.Map<Solution>(solutionSuggest)).Returns(solutionFromMapper);

        // Act
        await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetMessageByIdAsync(solutionSuggest.MessageId), Once);
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(message.SupportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: _ => _.AddSolutionAsync(It.Is<Solution>(solution =>
                solution == solutionFromMapper &&
                solution.Status == SolutionStatus.Suggested)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<Solution>(solutionSuggest));
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_MessageNotExists_Throw(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Message with id: {solutionSuggest.MessageId} not found");
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_SupportTicketNotOpen_Throw(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Arrange
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Close
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(message.SupportTicketId)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_AccountIsNotAgent_Throw(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Arrange
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(message.SupportTicketId)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, accountId);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_ExistsSuggestedSolution_Throw(SolutionSuggest solutionSuggest,
        Guid accountId)
    {
        // Arrange
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
        var supportTicket = new SupportTicket
        {
            Agent = new User
            {
                Id = accountId
            },
            Status = SupportTicketStatus.Open,
            Solutions = new[] { new Solution { Status = SolutionStatus.Suggested } }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(message.SupportTicketId)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Exists solution which already suggested");
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_MessageAlreadyWasSolution_Throw(SolutionSuggest solutionSuggest,
        Guid accountId)
    {
        // Arrange
        var message = new Message();
        _supportTicketsRepository.Setup(_ => _.GetMessageByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
        var supportTicket = new SupportTicket
        {
            Agent = new User
            {
                Id = accountId
            },
            Status = SupportTicketStatus.Open,
            Solutions = new[]
                { new Solution { MessageId = solutionSuggest.MessageId, Status = SolutionStatus.Rejected } }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(message.SupportTicketId)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"Message with id: {solutionSuggest.MessageId} was already solution");
    }

    [Test, AutoData]
    public async Task AcceptSolutionAsync_CorrectCase_Pass(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var solutionFromRepository = new Solution
        {
            Status = SolutionStatus.Suggested
        };
        var supportTicketFromRepository = new SupportTicket
        {
            User = new User { Id = accountId },
            Solutions = new[] { solutionFromRepository }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        await _supportTicketsService.AcceptSolutionAsync(supportTicketId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: _ => _.UpdateSolutionAsync(It.Is<Solution>(solution =>
                solution == solutionFromRepository && solution.Status == SolutionStatus.Accepted)),
            times: Once);
        _supportTicketsRepository.Verify(
            expression: _ => _.UpdateAsync(It.Is<SupportTicket>(supportTicket =>
                supportTicket == supportTicketFromRepository && supportTicket.Status == SupportTicketStatus.Solved)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _escalationsManager.Verify(_ => _.CancelEscalationFor(supportTicketId));
        _escalationsManager.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AcceptSolutionAsync_SupportTicketNotExists_Throw(Guid supportTicketId, Guid accountId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.AcceptSolutionAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task AcceptSolutionAsync_AccountIsNotUser_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            User = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AcceptSolutionAsync(supportTicketId, accountId);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task AcceptSolutionAsync_SuggestedSolutionNotExists_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            User = new User { Id = accountId },
            Solutions = Array.Empty<Solution>()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AcceptSolutionAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Suggested solution not exists");
    }

    [Test, AutoData]
    public async Task RejectSolutionAsync_CorrectCase_Pass(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var solutionFromRepository = new Solution
        {
            Status = SolutionStatus.Suggested
        };
        var supportTicketFromRepository = new SupportTicket
        {
            User = new User { Id = accountId },
            Solutions = new[] { solutionFromRepository }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        await _supportTicketsService.RejectSolutionAsync(supportTicketId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: _ => _.UpdateSolutionAsync(It.Is<Solution>(solution =>
                solution == solutionFromRepository && solution.Status == SolutionStatus.Rejected)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task RejectSolutionAsync_SupportTicketNotExists_Throw(Guid supportTicketId, Guid accountId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.RejectSolutionAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task RejectSolutionAsync_AccountIsNotUser_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            User = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.RejectSolutionAsync(supportTicketId, accountId);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task RejectSolutionAsync_SuggestedSolutionNotExists_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            User = new User { Id = accountId },
            Solutions = Array.Empty<Solution>()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.RejectSolutionAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Suggested solution not exists");
    }
}