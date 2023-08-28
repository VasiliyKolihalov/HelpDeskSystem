using Authentication.Infrastructure.Models;
using AutoMapper;
using Dapper;
using Infrastructure.Exceptions;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTicketAgentRecords;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.SupportTicketsPages;
using SupportTickets.WebApi.Models.SupportTicketStatusRecords;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.Messages;
using SupportTickets.WebApi.Repositories.Solutions;
using SupportTickets.WebApi.Repositories.SupportTicketAgentRecords;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.SupportTicketStatusRecords;
using SupportTickets.WebApi.Services;
using SupportTickets.WebApi.Services.Clients;
using SupportTickets.WebApi.Services.JobsManagers.Closing;
using SupportTickets.WebApi.Services.JobsManagers.Escalations;
using SupportTickets.WebApi.Services.SupportTicketsPaginationQueueBuilder;
using Message = SupportTickets.WebApi.Models.Messages.Message;

namespace SupportTickets.Tests;

public class Tests
{
    private Mock<ISupportTicketsRepository> _supportTicketsRepository = null!;
    private Mock<IMessagesRepository> _messagesRepository = null!;
    private Mock<ISolutionsRepository> _solutionsRepository = null!;
    private Mock<ISupportTicketAgentRecordsRepository> _agentRecordsRepository = null!;
    private Mock<ISupportTicketStatusRecordsRepository> _statusRecordsRepository = null!;
    private Mock<IResourcesGrpcClient> _resourcesGrpcClient = null!;
    private Mock<ISupportTicketsEscalationManager> _escalationManager = null!;
    private Mock<ISupportTicketsClosingManager> _closingManager = null!;
    private Mock<IMapper> _mapper = null!;
    private SupportTicketsService _supportTicketsService = null!;

    [SetUp]
    public void Setup()
    {
        _supportTicketsRepository = new Mock<ISupportTicketsRepository>();
        _messagesRepository = new Mock<IMessagesRepository>();
        _solutionsRepository = new Mock<ISolutionsRepository>();
        _agentRecordsRepository = new Mock<ISupportTicketAgentRecordsRepository>();
        _statusRecordsRepository = new Mock<ISupportTicketStatusRecordsRepository>();
        _resourcesGrpcClient = new Mock<IResourcesGrpcClient>();
        _escalationManager = new Mock<ISupportTicketsEscalationManager>();
        _closingManager = new Mock<ISupportTicketsClosingManager>();
        _mapper = new Mock<IMapper>();

        _supportTicketsService = new SupportTicketsService(
            _supportTicketsRepository.Object,
            _messagesRepository.Object,
            _solutionsRepository.Object,
            _agentRecordsRepository.Object,
            _statusRecordsRepository.Object,
            _resourcesGrpcClient.Object,
            _escalationManager.Object,
            _closingManager.Object,
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
    public async Task GetAllPageAsync_CorrectCase_Pass(SupportTicketPageGet pageGet, List<SupportTicket> supportTickets)
    {
        // Arrange
        _supportTicketsRepository
            .Setup(_ => _.GetByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()))
            .ReturnsAsync(supportTickets);
        _supportTicketsRepository
            .Setup(_ => _.GetCountByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()))
            .ReturnsAsync(supportTickets.Count);
        var page = new SupportTicketPageView();
        _mapper
            .Setup(_ => _.Map<SupportTicketPageView>(pageGet))
            .Returns(page);
        SupportTicketPreview[] supportTicketPreviewsFromMapper = Array.Empty<SupportTicketPreview>();
        _mapper
            .Setup(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets))
            .Returns(supportTicketPreviewsFromMapper);

        // Act
        SupportTicketPageView result = await _supportTicketsService.GetAllPageAsync(pageGet);

        // Assert
        result.TotalPages.Should().Be((int)Math.Ceiling((double)supportTickets.Count / pageGet.PageSize));
        result.SupportTickets.Should().BeEquivalentTo(supportTicketPreviewsFromMapper);
        _supportTicketsRepository.Verify(
            _ => _.GetByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()), Once);
        _supportTicketsRepository.Verify(
            _ => _.GetCountByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<SupportTicketPageView>(pageGet), Once);
        _mapper.Verify(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetFreeAsync_CorrectCase_Pass(Guid accountId, List<SupportTicket> supportTickets)
    {
        // Arrange
        _supportTicketsRepository.Setup(_ => _.GetAllOpenWithoutAgent()).ReturnsAsync(supportTickets);
        Guid supportTicketIdWhereAccountWasNotAgent = supportTickets.First().Id;
        _agentRecordsRepository
            .Setup(_ => _.GetBySupportTicketIdAsync(supportTicketIdWhereAccountWasNotAgent))
            .ReturnsAsync(Array.Empty<SupportTicketAgentRecord>());
        var agentRecords = new[] { new SupportTicketAgentRecord { AgentId = accountId } };
        _agentRecordsRepository
            .Setup(repository =>
                repository.GetBySupportTicketIdAsync(It.IsNotIn(supportTicketIdWhereAccountWasNotAgent)))
            .ReturnsAsync(agentRecords);

        // Act
        List<SupportTicketPreview> result = (await _supportTicketsService.GetFreeAsync(accountId)).AsList();

        // Assert
        result.Should().NotBeNull();
        _supportTicketsRepository.Verify(_ => _.GetAllOpenWithoutAgent(), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _agentRecordsRepository.Verify(
            expression: _ => _.GetBySupportTicketIdAsync(It.IsAny<Guid>()),
            times: Exactly(supportTickets.Count));
        _agentRecordsRepository.VerifyNoOtherCalls();
        _mapper.Verify(
            expression: mapper => mapper.Map<IEnumerable<SupportTicketPreview>>(
                It.Is<IEnumerable<SupportTicket>>(_ =>
                    _.Count() == 1 &&
                    _.First().Id == supportTicketIdWhereAccountWasNotAgent)),
            times: Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetFreePageAsync_CorrectCase_Pass(
        SupportTicketPageGetFree pageGetFree,
        Guid accountId,
        List<SupportTicket> supportTickets)
    {
        // Arrange
        _supportTicketsRepository
            .Setup(_ => _.GetByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()))
            .ReturnsAsync(supportTickets);
        _supportTicketsRepository
            .Setup(_ => _.GetCountByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()))
            .ReturnsAsync(supportTickets.Count);
        Guid supportTicketIdWhereAccountWasNotAgent = supportTickets.First().Id;
        _agentRecordsRepository
            .Setup(_ => _.GetBySupportTicketIdAsync(supportTicketIdWhereAccountWasNotAgent))
            .ReturnsAsync(Array.Empty<SupportTicketAgentRecord>());
        var agentRecords = new[] { new SupportTicketAgentRecord { AgentId = accountId } };
        _agentRecordsRepository
            .Setup(repository =>
                repository.GetBySupportTicketIdAsync(It.IsNotIn(supportTicketIdWhereAccountWasNotAgent)))
            .ReturnsAsync(agentRecords);
        var page = new SupportTicketPageView();
        _mapper
            .Setup(_ => _.Map<SupportTicketPageView>(pageGetFree))
            .Returns(page);
        SupportTicketPreview[] supportTicketPreviewsFromMapper = Array.Empty<SupportTicketPreview>();
        _mapper
            .Setup(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets))
            .Returns(supportTicketPreviewsFromMapper);

        // Act
        SupportTicketPageView result = await _supportTicketsService.GetFreePageAsync(pageGetFree, accountId);

        // Assert
        result.TotalPages.Should().Be((int)Math.Ceiling((double)supportTickets.Count / pageGetFree.PageSize));
        result.SupportTickets.Should().BeEquivalentTo(supportTicketPreviewsFromMapper);
        result.Status.Should().Be(SupportTicketStatus.Open);
        _supportTicketsRepository.Verify(
            _ => _.GetByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()), Once);
        _supportTicketsRepository.Verify(
            _ => _.GetCountByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _agentRecordsRepository.Verify(
            expression: _ => _.GetBySupportTicketIdAsync(It.IsAny<Guid>()),
            times: Exactly(supportTickets.Count));
        _agentRecordsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<SupportTicketPageView>(pageGetFree), Once);
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
        Guid accountId,
        IEnumerable<SupportTicket> supportTickets)
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
    public async Task GetByAccountIdPageAsync_CorrectCase_Pass(
        SupportTicketPageGet pageGet,
        Guid accountId,
        List<SupportTicket> supportTickets)
    {
        // Arrange
        _supportTicketsRepository
            .Setup(_ => _.GetByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()))
            .ReturnsAsync(supportTickets);
        _supportTicketsRepository
            .Setup(_ => _.GetCountByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()))
            .ReturnsAsync(supportTickets.Count);
        var page = new SupportTicketPageView();
        _mapper
            .Setup(_ => _.Map<SupportTicketPageView>(pageGet))
            .Returns(page);
        SupportTicketPreview[] supportTicketPreviewsFromMapper = Array.Empty<SupportTicketPreview>();
        _mapper
            .Setup(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets))
            .Returns(supportTicketPreviewsFromMapper);

        // Act
        SupportTicketPageView result = await _supportTicketsService.GetByAccountIdPageAsync(pageGet, accountId);

        // Assert
        result.TotalPages.Should().Be((int)Math.Ceiling((double)supportTickets.Count / pageGet.PageSize));
        result.SupportTickets.Should().BeEquivalentTo(supportTicketPreviewsFromMapper);
        _supportTicketsRepository.Verify(
            _ => _.GetByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()), Once);
        _supportTicketsRepository.Verify(
            _ => _.GetCountByPagination(It.IsAny<Action<ISupportTicketsPaginationQueueBuilder>>()), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<SupportTicketPageView>(pageGet), Once);
        _mapper.Verify(_ => _.Map<IEnumerable<SupportTicketPreview>>(supportTickets), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetByIdAsync_CorrectCase_Pass(SupportTicket supportTicket, Guid accountId)
    {
        // Arrange
        supportTicket.User = new User { Id = accountId };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new MessageView();
        var supportTicketViewFromMapper = new SupportTicketView
        {
            Messages = new[] { message }
        };
        _mapper.Setup(_ => _.Map<SupportTicketView>(supportTicket)).Returns(supportTicketViewFromMapper);
        var account = new Account<Guid> { Id = accountId };

        // Act
        await _supportTicketsService.GetByIdAsync(supportTicket.Id, account);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _resourcesGrpcClient.Verify(_ => _.SendGetMessageImagesRequestAsync(message.Id), Once);
        _resourcesGrpcClient.VerifyNoOtherCalls();
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
        var recordFromMapper = new SupportTicketStatusRecord();
        _mapper.Setup(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromMapper)).Returns(recordFromMapper);

        // Act
        Guid result = await _supportTicketsService.CreateAsync(supportTicketCreate, account);

        // Assert
        result.Should().NotBeEmpty();
        _mapper.Verify(_ => _.Map<SupportTicket>(supportTicketCreate), Once);
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.Verify(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromMapper), Once);
        _mapper.VerifyNoOtherCalls();
        _supportTicketsRepository.Verify(
            expression: repository =>
                repository.InsertAsync(It.Is<SupportTicket>(supportTicket =>
                    supportTicket == supportTicketFromMapper &&
                    supportTicket.User.Id == userFromMapper.Id &&
                    supportTicket.Status == SupportTicketStatus.Open)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _statusRecordsRepository.Verify(
            expression: _ => _.InsertAsync(It.Is<SupportTicketStatusRecord>(
                record => record == recordFromMapper && record.DateTime.AddMinutes(1) > DateTime.Now)),
            times: Once);
        _statusRecordsRepository.VerifyNoOtherCalls();
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
            Status = SupportTicketStatus.Open,
            Solutions = Array.Empty<Solution>()
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
    public async Task UpdateAsync_AccountIsNotRelated_Throw(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateAsync(supportTicketUpdate, account);

        // Result
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task UpdateAsync_SupportTicketNotOpen_Throw(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User { Id = account.Id },
            Status = SupportTicketStatus.Close
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService
            .UpdateAsync(supportTicketUpdate, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
    }

    [Test, AutoData]
    public async Task UpdateAsync_SupportTicketHaveSuggestedSolution_Throw(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User { Id = account.Id },
            Status = SupportTicketStatus.Open,
            Solutions = new[] { new Solution { Status = SolutionStatus.Suggested } }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketUpdate.Id)).ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService
            .UpdateAsync(supportTicketUpdate, account);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} have suggested solution");
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
    public async Task GetAgentHistoryAsync_CorrectCase_Pass(Guid id)
    {
        // Arrange
        SupportTicketAgentRecord[] records = Array.Empty<SupportTicketAgentRecord>();
        _agentRecordsRepository.Setup(_ => _.GetBySupportTicketIdAsync(id)).ReturnsAsync(records);

        // Act
        IEnumerable<SupportTicketAgentRecordView> result = await _supportTicketsService.GetAgentHistoryAsync(id);

        // Assert
        result.Should().NotBeNull();
        _agentRecordsRepository.Verify(_ => _.GetBySupportTicketIdAsync(id), Once);
        _agentRecordsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<IEnumerable<SupportTicketAgentRecordView>>(records), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AppointAgentAsync_CorrectCase_Pass(Guid supportTicketId, Account<Guid> account)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket();
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId))
            .ReturnsAsync(supportTicketFromRepository);
        _agentRecordsRepository
            .Setup(_ => _.GetBySupportTicketIdAsync(supportTicketId))
            .ReturnsAsync(Array.Empty<SupportTicketAgentRecord>());
        var user = new User();
        _mapper.Setup(_ => _.Map<User>(account)).Returns(user);
        var recordFromMapper = new SupportTicketAgentRecord();
        _mapper
            .Setup(_ => _.Map<SupportTicketAgentRecord>(supportTicketFromRepository))
            .Returns(recordFromMapper);

        // Act
        await _supportTicketsService.AppointAgentAsync(supportTicketId, account);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: repository =>
                repository.UpdateAsync(It.Is<SupportTicket>(supportTicket =>
                    supportTicket == supportTicketFromRepository &&
                    supportTicket.Priority == SupportTicketPriority.Low &&
                    supportTicket.Agent == user)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _agentRecordsRepository.Verify(_ => _.GetBySupportTicketIdAsync(supportTicketId), Once);
        _agentRecordsRepository.Verify(
            expression: _ => _.InsertAsync(It.Is<SupportTicketAgentRecord>(record =>
                record == recordFromMapper && record.DateTime.AddMinutes(1) > DateTime.Now)),
            times: Once);
        _agentRecordsRepository.VerifyNoOtherCalls();
        _escalationManager.Verify(_ => _.AssignEscalationFor(supportTicketId, TimeSpan.FromDays(10)), Once);
        _escalationManager.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.Verify(_ => _.Map<SupportTicketAgentRecord>(supportTicketFromRepository), Once);
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
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
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
        var records = new[] { new SupportTicketAgentRecord { AgentId = account.Id } };
        _agentRecordsRepository
            .Setup(_ => _.GetBySupportTicketIdAsync(supportTicketId))
            .ReturnsAsync(records);

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
    public async Task AddMessageAsync_UserAccount_Pass(MessageCreate messageCreate, Account<Guid> account)
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
        _messagesRepository.Verify(repository =>
                repository.InsertAsync(It.Is<Message>(message =>
                    message == messageFromMapper &&
                    message.User.Id == account.Id)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _resourcesGrpcClient.Verify(_ => _.SendAddImagesToMessageRequestAsync(messageCreate.Images!, messageFromMapper.Id));
        _resourcesGrpcClient.VerifyNoOtherCalls();
        _closingManager.Verify(_ => _.EnsureCancelCloseFor(messageCreate.SupportTicketId), Once);
        _closingManager.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<Message>(messageCreate), Once);
        _mapper.Verify(_ => _.Map<User>(account), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AddMessageAsync_AgentAccount_Pass(MessageCreate messageCreate, Account<Guid> account)
    {
        // Arrange
        var messageFromMapper = new Message();
        _mapper.Setup(_ => _.Map<Message>(messageCreate)).Returns(messageFromMapper);
        var user = new User { Id = account.Id };
        _mapper.Setup(_ => _.Map<User>(account)).Returns(user);
        var supportTicket = new SupportTicket
        {
            User = new User(),
            Agent = user,
            Status = SupportTicketStatus.Open
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Guid result = await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        result.Should().NotBeEmpty();
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(messageCreate.SupportTicketId), Once);
        _messagesRepository.Verify(repository =>
                repository.InsertAsync(It.Is<Message>(message =>
                    message == messageFromMapper &&
                    message.User.Id == account.Id)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _closingManager.Verify(_ => _.EnsureAssignCloseFor(messageCreate.SupportTicketId, TimeSpan.FromDays(1)), Once);
        _closingManager.VerifyNoOtherCalls();
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
    public async Task AddMessageAsync_AccountIsNotRelated_Throw(
        MessageCreate messageCreate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(messageCreate.SupportTicketId))
            .ReturnsAsync(supportTicket);

        // Act
        Func<Task> action = async () => await _supportTicketsService.AddMessageAsync(messageCreate, account);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task AddMessageAsync_SupportTicketNotOpen_Throw(
        MessageCreate messageCreate,
        Account<Guid> account)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            User = new User { Id = account.Id },
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
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
    }

    [Test, AutoData]
    public async Task UpdateMessageAsync_CorrectCase_Pass(MessageUpdate messageUpdate, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            Solutions = Array.Empty<Solution>()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _messagesRepository.Setup(_ => _.GetByIdAsync(messageUpdate.Id)).ReturnsAsync(message);
        var updatedMessage = new Message();
        _mapper.Setup(_ => _.Map<Message>(messageUpdate)).Returns(updatedMessage);


        // Act
        await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _messagesRepository.Verify(_ => _.GetByIdAsync(messageUpdate.Id), Once);
        _messagesRepository.Verify(_ => _.UpdateAsync(updatedMessage), Once);
        _messagesRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<Message>(messageUpdate), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdateMessageAsync_MessageNotExists_Throw(MessageUpdate messageUpdate, Guid accountId)
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
    public async Task UpdateMessageAsync_AccountIsNotRelated_Throw(MessageUpdate messageUpdate, Guid accountId)
    {
        // Arrange
        var message = new Message
        {
            User = new User()
        };
        _messagesRepository
            .Setup(_ => _.GetByIdAsync(messageUpdate.Id))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<UnauthorizedException>();
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
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _messagesRepository.Setup(_ => _.GetByIdAsync(messageUpdate.Id))
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
    public async Task UpdateMessageAsync_SupportTicketHaveSuggestedSolution_Throw(
        MessageUpdate messageUpdate,
        Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            Solutions = new[] { new Solution { Status = SolutionStatus.Suggested } }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _messagesRepository.Setup(_ => _.GetByIdAsync(messageUpdate.Id))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.UpdateMessageAsync(messageUpdate, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} have suggested solution");
    }

    [Test, AutoData]
    public async Task DeleteMessageAsync_CorrectCase_Pass(Guid messageId, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            Solutions = Array.Empty<Solution>()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _messagesRepository
            .Setup(_ => _.GetByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicket.Id), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _messagesRepository.Verify(_ => _.GetByIdAsync(messageId), Once);
        _messagesRepository.Verify(_ => _.DeleteAsync(messageId), Once);
        _messagesRepository.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task DeleteMessageAsync_MessageNotExists_Throw(Guid messageId, Guid accountId)
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
    public async Task DeleteMessageAsync_AccountIsNotRelated_Throw(Guid messageId, Guid accountId)
    {
        // Arrange
        var message = new Message
        {
            User = new User()
        };
        _messagesRepository.Setup(_ => _.GetByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<UnauthorizedException>();
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
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _messagesRepository
            .Setup(_ => _.GetByIdAsync(messageId))
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
    public async Task DeleteMessageAsync_SupportTicketHaveSuggestedSolution_Throw(Guid messageId, Guid accountId)
    {
        // Arrange
        var supportTicket = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            Solutions = new[] { new Solution { Status = SolutionStatus.Suggested } }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicket.Id)).ReturnsAsync(supportTicket);
        var message = new Message
        {
            User = new User { Id = accountId }
        };
        _messagesRepository
            .Setup(_ => _.GetByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        Func<Task> action = async () => await _supportTicketsService.DeleteMessageAsync(messageId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id: {supportTicket.Id} have suggested solution");
    }

    [Test, AutoData]
    public async Task GetStatusHistoryAsync_CorrectCase_Pass(Guid id)
    {
        // Arrange
        SupportTicketStatusRecord[] records = Array.Empty<SupportTicketStatusRecord>();
        _statusRecordsRepository.Setup(_ => _.GetBySupportTicketIdAsync(id)).ReturnsAsync(records);

        // Act
        IEnumerable<SupportTicketStatusRecordView> result = await _supportTicketsService.GetStatusHistoryAsync(id);

        // Assert
        result.Should().NotBeNull();
        _statusRecordsRepository.Verify(_ => _.GetBySupportTicketIdAsync(id), Once);
        _statusRecordsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<IEnumerable<SupportTicketStatusRecordView>>(records), Once);
        _mapper.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_CorrectCase_Pass(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Arrange
        var message = new Message();
        _messagesRepository.Setup(_ => _.GetByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
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
        _messagesRepository.Verify(_ => _.GetByIdAsync(solutionSuggest.MessageId), Once);
        _messagesRepository.VerifyNoOtherCalls();
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(message.SupportTicketId), Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _solutionsRepository.Verify(
            expression: _ => _.InsertAsync(It.Is<Solution>(solution =>
                solution == solutionFromMapper &&
                solution.Status == SolutionStatus.Suggested)),
            times: Once);
        _solutionsRepository.VerifyNoOtherCalls();
        _closingManager.Verify(_ => _.EnsureAssignCloseFor(message.SupportTicketId, TimeSpan.FromDays(1)), Once);
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
    public async Task SuggestSolutionAsync_AccountIsNotAgent_Throw(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Arrange
        var message = new Message();
        _messagesRepository.Setup(_ => _.GetByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
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
    public async Task SuggestSolutionAsync_SupportTicketNotOpen_Throw(SolutionSuggest solutionSuggest, Guid accountId)
    {
        // Arrange
        var message = new Message();
        _messagesRepository.Setup(_ => _.GetByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
        var supportTicket = new SupportTicket
        {
            Agent = new User { Id = accountId },
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
    public async Task SuggestSolutionAsync_SupportTicketHaveSuggestedSolution_Throw(SolutionSuggest solutionSuggest,
        Guid accountId)
    {
        // Arrange
        var message = new Message();
        _messagesRepository.Setup(_ => _.GetByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
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
            .WithMessage($"SupportTicket with id: {supportTicket.Id} have suggested solution");
    }

    [Test, AutoData]
    public async Task SuggestSolutionAsync_MessageAlreadyWasSolution_Throw(SolutionSuggest solutionSuggest,
        Guid accountId)
    {
        // Arrange
        var message = new Message();
        _messagesRepository.Setup(_ => _.GetByIdAsync(solutionSuggest.MessageId)).ReturnsAsync(message);
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
        var recordFromMapper = new SupportTicketStatusRecord();
        _mapper.Setup(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromRepository)).Returns(recordFromMapper);

        // Act
        await _supportTicketsService.AcceptSolutionAsync(supportTicketId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: _ => _.UpdateAsync(It.Is<SupportTicket>(supportTicket =>
                supportTicket == supportTicketFromRepository && supportTicket.Status == SupportTicketStatus.Solved)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _solutionsRepository.Verify(
            expression: _ => _.UpdateAsync(It.Is<Solution>(solution =>
                solution == solutionFromRepository && solution.Status == SolutionStatus.Accepted)),
            times: Once);
        _solutionsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromRepository), Once);
        _mapper.VerifyNoOtherCalls();
        _statusRecordsRepository.Verify(
            expression: _ => _.InsertAsync(It.Is<SupportTicketStatusRecord>(record =>
                record == recordFromMapper && record.DateTime.AddMinutes(1) > DateTime.Now)),
            times: Once);
        _statusRecordsRepository.VerifyNoOtherCalls();
        _escalationManager.Verify(_ => _.CancelEscalationFor(supportTicketId), Once);
        _escalationManager.VerifyNoOtherCalls();
        _closingManager.Verify(_ => _.EnsureCancelCloseFor(supportTicketId), Once);
        _closingManager.VerifyNoOtherCalls();
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
        _supportTicketsRepository.VerifyNoOtherCalls();
        _solutionsRepository.Verify(
            expression: _ => _.UpdateAsync(It.Is<Solution>(solution =>
                solution == solutionFromRepository && solution.Status == SolutionStatus.Rejected)),
            times: Once);
        _solutionsRepository.VerifyNoOtherCalls();
        _closingManager.Verify(_ => _.EnsureCancelCloseFor(supportTicketId));
        _closingManager.VerifyNoOtherCalls();
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
        var recordFromMapper = new SupportTicketStatusRecord();
        _mapper.Setup(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromRepository)).Returns(recordFromMapper);

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
        _mapper.Verify(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromRepository), Once);
        _mapper.VerifyNoOtherCalls();
        _statusRecordsRepository.Verify(
            expression: _ => _.InsertAsync(It.Is<SupportTicketStatusRecord>(record =>
                record == recordFromMapper && record.DateTime.AddMinutes(1) > DateTime.Now)),
            times: Once);
        _statusRecordsRepository.VerifyNoOtherCalls();
        _escalationManager.Verify(_ => _.CancelEscalationFor(supportTicketId));
        _escalationManager.VerifyNoOtherCalls();
        _closingManager.Verify(_ => _.EnsureCancelCloseFor(supportTicketId));
        _closingManager.VerifyNoOtherCalls();
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
            .WithMessage($"SupportTicket with id: {supportTicket.Id} not open");
    }

    [Test, AutoData]
    public async Task ReopenAsync_CorrectCase_Pass(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            Status = SupportTicketStatus.Close,
            User = new User { Id = accountId }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);
        var records = new[] { new SupportTicketStatusRecord { DateTime = DateTime.Now } };
        _statusRecordsRepository
            .Setup(_ => _.GetBySupportTicketIdAsync(supportTicketId))
            .ReturnsAsync(records);
        var recordFromMapper = new SupportTicketStatusRecord();
        _mapper.Setup(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromRepository)).Returns(recordFromMapper);

        // Act
        await _supportTicketsService.ReopenAsync(supportTicketId, accountId);

        // Assert
        _supportTicketsRepository.Verify(_ => _.GetByIdAsync(supportTicketId), Once);
        _supportTicketsRepository.Verify(
            expression: repository =>
                repository.UpdateAsync(It.Is<SupportTicket>(supportTicket =>
                    supportTicket == supportTicketFromRepository &&
                    supportTicket.Status == SupportTicketStatus.Open)),
            times: Once);
        _supportTicketsRepository.VerifyNoOtherCalls();
        _mapper.Verify(_ => _.Map<SupportTicketStatusRecord>(supportTicketFromRepository), Once);
        _mapper.VerifyNoOtherCalls();
        _statusRecordsRepository.Verify(_ => _.GetBySupportTicketIdAsync(supportTicketId), Once);
        _statusRecordsRepository.Verify(
            expression: _ => _.InsertAsync(It.Is<SupportTicketStatusRecord>(record =>
                record == recordFromMapper && record.DateTime.AddMinutes(1) > DateTime.Now)),
            times: Once);
        _statusRecordsRepository.VerifyNoOtherCalls();
        _escalationManager.Verify(_ => _.AssignEscalationFor(supportTicketId, TimeSpan.FromDays(10)));
        _escalationManager.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task ReopenAsync_SupportTicketNotFound_Throw(Guid supportTicketId, Guid accountId)
    {
        // Act
        Func<Task> action = async () => await _supportTicketsService.ReopenAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"SupportTicket with id: {supportTicketId} not found");
    }

    [Test, AutoData]
    public async Task ReopenAsync_AccountIsNotRelated_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            User = new User()
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.ReopenAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }

    [Test, AutoData]
    public async Task ReopenAsync_SupportTicketNotClose_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            Status = SupportTicketStatus.Open,
            User = new User { Id = accountId }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);

        // Act
        Func<Task> action = async () => await _supportTicketsService.ReopenAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket with id {supportTicketId} not close");
    }

    [Test, AutoData]
    public async Task ReopenAsync_TimeForReopenElapsed_Throw(Guid supportTicketId, Guid accountId)
    {
        // Arrange
        var supportTicketFromRepository = new SupportTicket
        {
            Status = SupportTicketStatus.Close,
            User = new User { Id = accountId }
        };
        _supportTicketsRepository.Setup(_ => _.GetByIdAsync(supportTicketId)).ReturnsAsync(supportTicketFromRepository);
        var records = new[]
            { new SupportTicketStatusRecord { DateTime = DateTime.Now.Subtract(TimeSpan.FromDays(20)) } };
        _statusRecordsRepository
            .Setup(_ => _.GetBySupportTicketIdAsync(supportTicketId))
            .ReturnsAsync(records);

        // Act
        Func<Task> action = async () => await _supportTicketsService.ReopenAsync(supportTicketId, accountId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage($"SupportTicket can be reopen only if less elapsed than {TimeSpan.FromDays(10)}");
    }
}