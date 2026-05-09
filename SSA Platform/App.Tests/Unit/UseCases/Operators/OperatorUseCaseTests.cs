// ═════════════════════════════════════════════════════════════════════════════

using App.Application.Features.Operators.DTOs;
using App.Application.Features.Operators.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Application.Mappers;
using App.Application.Mappers.Notifications;
using App.Application.Mappers.Operator;
using App.Application.Mappers.Satellites;
using App.Application.Mappers.Shared;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.Entities.Identity;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace App.Tests.Unit.UseCases.Operators;

public class AddHelperUseCaseTests : OperatorUseCaseTestBase
{
    private readonly AddHelperUseCase _sut;

    public AddHelperUseCaseTests()
    {
        _sut = new AddHelperUseCase(Uow.Object, UserManager.Object);
    }

    private static AddHelperRequest ValidRequest(string email = "helper@example.com") =>
        new AddHelperRequest { Email = email };

    // ── Guard clauses ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        Func<Task> act = () => _sut.ExecuteAsync(Guid.NewGuid(), ValidRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No user found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserAlreadyHasAccess_ThrowsException()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync(new UserOperator { AppUserId = user.Id, OperatorId = operatorId });

        Func<Task> act = () => _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!));

        await act.Should().ThrowAsync<Exception>().WithMessage("*already has access*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserAlreadyHasAccess_DoesNotCommit()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync(new UserOperator { AppUserId = user.Id, OperatorId = operatorId });

        try { await _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!)); } catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Role assignment ───────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenUserNotAlreadyInRole_AssignsOperatorHelperRole()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync((UserOperator?)null);
        UserManager.Setup(m => m.IsInRoleAsync(user, "OperatorHelper")).ReturnsAsync(false);
        UserManager.Setup(m => m.RemoveFromRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);
        UserManager.Setup(m => m.AddToRoleAsync(user, "OperatorHelper")).ReturnsAsync(IdentityResult.Success);

        await _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!));

        UserManager.Verify(m => m.AddToRoleAsync(user, "OperatorHelper"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserAlreadyInRole_SkipsRoleAssignment()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync((UserOperator?)null);
        UserManager.Setup(m => m.IsInRoleAsync(user, "OperatorHelper")).ReturnsAsync(true);

        await _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!));

        UserManager.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRoleAssignmentFails_ThrowsException()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync((UserOperator?)null);
        UserManager.Setup(m => m.IsInRoleAsync(user, "OperatorHelper")).ReturnsAsync(false);
        UserManager.Setup(m => m.RemoveFromRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);
        UserManager
            .Setup(m => m.AddToRoleAsync(user, "OperatorHelper"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role error" }));

        Func<Task> act = () => _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!));

        await act.Should().ThrowAsync<Exception>().WithMessage("*Failed to assign role*");
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_AddsUserOperator()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync((UserOperator?)null);
        UserManager.Setup(m => m.IsInRoleAsync(user, "OperatorHelper")).ReturnsAsync(true);

        await _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!));

        UserOperatorRepo.Verify(
            r => r.Add(It.Is<UserOperator>(uo =>
                    uo.AppUserId == user.Id && uo.OperatorId == operatorId),
                It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CommitsUnitOfWork()
    {
        var user       = MakeUser();
        var operatorId = Guid.NewGuid();
        UserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        UserOperatorRepo
            .Setup(r => r.GetByUserAndOperatorAsync(user.Id, operatorId))
            .ReturnsAsync((UserOperator?)null);
        UserManager.Setup(m => m.IsInRoleAsync(user, "OperatorHelper")).ReturnsAsync(true);

        await _sut.ExecuteAsync(operatorId, ValidRequest(user.Email!));

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class AddOperatorUseCaseTests : OperatorUseCaseTestBase
{
    private readonly AddOperatorUseCase _sut;

    public AddOperatorUseCaseTests()
    {
        _sut = new AddOperatorUseCase(Uow.Object);
    }

    private static AddOperatorRequest ValidRequest() => new AddOperatorRequest
    {
        Name         = "SpaceCorp",
        Country      = "US",
        OperatorType = EOperatorType.Commercial
    };

    [Fact]
    public async Task Execute_WithValidRequest_AddsOperatorToRepository()
    {
        await _sut.Execute(ValidRequest());

        OperatorRepo.Verify(
            r => r.Add(It.IsAny<Operator>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CommitsUnitOfWork()
    {
        await _sut.Execute(ValidRequest());

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_ReturnsNonEmptyGuid()
    {
        var id = await _sut.Execute(ValidRequest());

        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Execute_CalledTwice_ReturnsDifferentIds()
    {
        var id1 = await _sut.Execute(ValidRequest());
        var id2 = await _sut.Execute(ValidRequest());

        id1.Should().NotBe(id2);
    }

    [Fact]
    public async Task Execute_WithValidRequest_OperatorHasCorrectName()
    {
        var request = ValidRequest();
        Operator? added = null;
        OperatorRepo
            .Setup(r => r.Add(It.IsAny<Operator>(), It.IsAny<Guid>()))
            .Callback<Operator, Guid>((o, _) => added = o);

        await _sut.Execute(request);

        added!.Name.Translate().Should().Be(request.Name);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class GetSettingsUseCaseTests : OperatorUseCaseTestBase
{
    private readonly GetSettingsUseCase _sut;

    public GetSettingsUseCaseTests()
    {
        var orbitMapper        = new OrbitMapper();
        var satelliteMapper    = new SatelliteSummaryMapper(orbitMapper);
        var operatorMapper     = new OperatorSummaryMapper();
        var notificationMapper = new NotificationMapper();

        _sut = new GetSettingsUseCase(Uow.Object, operatorMapper, satelliteMapper, notificationMapper);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorNotFound_ThrowsException()
    {
        OperatorRepo
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Operator?)null);

        Func<Task> act = () => _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFound_ReturnsDtoWithCorrectOperatorId()
    {
        var op = MakeOperator();
        OperatorRepo.Setup(r => r.GetWithDetailsAsync(op.Id)).ReturnsAsync(op);
        NotificationRepo
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((NotificationSetting?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), op.Id);

        result.Operator!.Id.Should().Be(op.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoNotificationSetting_NotificationSettingDtoIsNull()
    {
        var op = MakeOperator();
        OperatorRepo.Setup(r => r.GetWithDetailsAsync(op.Id)).ReturnsAsync(op);
        NotificationRepo
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((NotificationSetting?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), op.Id);

        result.NotificationSetting.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotificationSettingExists_MapsThresholds()
    {
        var op = MakeOperator();
        var setting = new NotificationSetting
        {
            Id                     = Guid.NewGuid(),
            AppUserId              = Guid.NewGuid(),
            ConjunctionThresholdKm = 50.0,
            AlertThresholdKm       = 5.0,
            EmailEnabled           = true,
            SmsEnabled             = false,
            CreatedBy              = "system",
            CreatedAt              = DateTime.UtcNow
        };
        OperatorRepo.Setup(r => r.GetWithDetailsAsync(op.Id)).ReturnsAsync(op);
        NotificationRepo
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(setting);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), op.Id);

        result.NotificationSetting!.ConjunctionThresholdKm.Should().Be(50.0);
        result.NotificationSetting.AlertThresholdKm.Should().Be(5.0);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class GetUserContextUseCaseTests : OperatorUseCaseTestBase
{
    private readonly GetUserContextUseCase _sut;

    public GetUserContextUseCaseTests()
    {
        _sut = new GetUserContextUseCase(Uow.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    public async Task ExecuteAsync_WhenClaimIsInvalid_ReturnsBothEmpty(string? claim)
    {
        var result = await _sut.ExecuteAsync(claim);

        result.UserId.Should().Be(Guid.Empty);
        result.OperatorId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoUserOperatorFound_ReturnsBothEmpty()
    {
        var userId = Guid.NewGuid();
        UserOperatorRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((UserOperator?)null);

        var result = await _sut.ExecuteAsync(userId.ToString());

        result.UserId.Should().Be(Guid.Empty);
        result.OperatorId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserOperatorFound_ReturnsCorrectUserIdAndOperatorId()
    {
        var userId     = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        UserOperatorRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new UserOperator { AppUserId = userId, OperatorId = operatorId });

        var result = await _sut.ExecuteAsync(userId.ToString());

        result.UserId.Should().Be(userId);
        result.OperatorId.Should().Be(operatorId);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class RegisterOperatorUseCaseTests : OperatorUseCaseTestBase
{
    private readonly RegisterOperatorUseCase _sut;

    public RegisterOperatorUseCaseTests()
    {
        _sut = new RegisterOperatorUseCase(Uow.Object, UserManager.Object);
    }

    private static RegisterOperatorRequest ValidUserRequest() => new RegisterOperatorRequest
    {
        Email              = "user@example.com",
        Password           = "Password123!",
        FirstName          = "John",
        LastName           = "Doe",
        RegisterAsOperator = false
    };

    private static RegisterOperatorRequest ValidOperatorRequest() => new RegisterOperatorRequest
    {
        Email              = "op@example.com",
        Password           = "Password123!",
        FirstName          = "Jane",
        LastName           = "Smith",
        RegisterAsOperator = true,
        OperatorName       = "SpaceCorp",
        OperatorCountry    = "US",
        OperatorType       = EOperatorType.Commercial
    };

    // ── Email check ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenEmailAlreadyRegistered_ReturnsFailure()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new AppUser());

        var (success, _) = await _sut.Execute(ValidUserRequest());

        success.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenEmailAlreadyRegistered_ErrorMentionsEmail()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new AppUser());

        var (_, error) = await _sut.Execute(ValidUserRequest());

        error.Should().Contain("already registered");
    }

    // ── Operator field validation ─────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenRegisterAsOperatorWithoutName_ReturnsFailure()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        var request = ValidOperatorRequest();
        request.OperatorName = "";

        var (success, error) = await _sut.Execute(request);

        success.Should().BeFalse();
        error.Should().Contain("name is required");
    }

    [Fact]
    public async Task Execute_WhenRegisterAsOperatorWithoutCountry_ReturnsFailure()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        var request = ValidOperatorRequest();
        request.OperatorCountry = "";

        var (success, error) = await _sut.Execute(request);

        success.Should().BeFalse();
        error.Should().Contain("country is required");
    }

    // ── CreateAsync failure ───────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenCreateUserFails_ReturnsFailureWithDescription()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        UserManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password" }));

        var (success, error) = await _sut.Execute(ValidUserRequest());

        success.Should().BeFalse();
        error.Should().Contain("Weak password");
    }

    // ── Happy path: plain user ────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenNotRegisteringAsOperator_ReturnsSuccess()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        UserManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var (success, error) = await _sut.Execute(ValidUserRequest());

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task Execute_WhenNotRegisteringAsOperator_DoesNotAddRoleOrOperator()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        UserManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Execute(ValidUserRequest());

        UserManager.Verify(
            m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Operator"),
            Times.Never);
        OperatorRepo.Verify(
            r => r.Add(It.IsAny<Operator>(), It.IsAny<Guid>()),
            Times.Never);
    }

    // ── Happy path: operator registration ────────────────────────────────────

    [Fact]
    public async Task Execute_WhenRegisteringAsOperator_AssignsOperatorRole()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        UserManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Operator"))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Execute(ValidOperatorRequest());

        UserManager.Verify(
            m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Operator"),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenRegisteringAsOperator_AddsOperatorAndUserOperator()
    {
        var mockUserOperatorRepo = new Mock<IUserOperatorRepository>();
        Uow.Setup(u => u.UserOperators).Returns(mockUserOperatorRepo.Object);

        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        UserManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Operator"))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Execute(ValidOperatorRequest());

        OperatorRepo.Verify(r => r.Add(It.IsAny<Operator>(), It.IsAny<Guid>()), Times.Once);
        mockUserOperatorRepo.Verify(r => r.Add(It.IsAny<UserOperator>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenRoleAssignmentFails_DeletesUserAndReturnsFailure()
    {
        UserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        UserManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Operator"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role error" }));
        UserManager
            .Setup(m => m.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var (success, _) = await _sut.Execute(ValidOperatorRequest());

        success.Should().BeFalse();
        UserManager.Verify(m => m.DeleteAsync(It.IsAny<AppUser>()), Times.Once);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class RemoveHelperUseCaseTests : OperatorUseCaseTestBase
{
    private readonly RemoveHelperUseCase _sut;

    public RemoveHelperUseCaseTests()
    {
        _sut = new RemoveHelperUseCase(Uow.Object);
    }

    private static UserOperator MakeUserOperator(Guid operatorId, Guid? userId = null) =>
        new UserOperator
        {
            Id         = Guid.NewGuid(),
            OperatorId = operatorId,
            AppUserId  = userId ?? Guid.NewGuid()
        };

    // ── Guard clauses ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenHelperNotFound_ThrowsException()
    {
        UserOperatorRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((UserOperator?)null);

        Func<Task> act = () =>
            _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenHelperBelongsToOtherOperator_ThrowsUnauthorizedAccessException()
    {
        var operatorId   = Guid.NewGuid();
        var userOperator = MakeUserOperator(operatorId: Guid.NewGuid()); // different operator
        UserOperatorRepo
            .Setup(r => r.FindAsync(userOperator.Id, It.IsAny<Guid>()))
            .ReturnsAsync(userOperator);

        Func<Task> act = () =>
            _sut.ExecuteAsync(userOperator.Id, operatorId, Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRemovingSelf_ThrowsException()
    {
        var operatorId   = Guid.NewGuid();
        var userId       = Guid.NewGuid();
        var userOperator = MakeUserOperator(operatorId, userId);
        UserOperatorRepo
            .Setup(r => r.FindAsync(userOperator.Id, It.IsAny<Guid>()))
            .ReturnsAsync(userOperator);

        Func<Task> act = () =>
            _sut.ExecuteAsync(userOperator.Id, operatorId, requestingUserId: userId);

        await act.Should().ThrowAsync<Exception>().WithMessage("*cannot remove yourself*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenGuardFails_DoesNotCommit()
    {
        UserOperatorRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((UserOperator?)null);

        try { await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()); }
        catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_RemovesUserOperator()
    {
        var operatorId   = Guid.NewGuid();
        var userOperator = MakeUserOperator(operatorId);
        UserOperatorRepo
            .Setup(r => r.FindAsync(userOperator.Id, It.IsAny<Guid>()))
            .ReturnsAsync(userOperator);

        await _sut.ExecuteAsync(userOperator.Id, operatorId, Guid.NewGuid());

        UserOperatorRepo.Verify(r => r.Remove(userOperator), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CommitsUnitOfWork()
    {
        var operatorId   = Guid.NewGuid();
        var userOperator = MakeUserOperator(operatorId);
        UserOperatorRepo
            .Setup(r => r.FindAsync(userOperator.Id, It.IsAny<Guid>()))
            .ReturnsAsync(userOperator);

        await _sut.ExecuteAsync(userOperator.Id, operatorId, Guid.NewGuid());

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class UpdateOperatorSettingsUseCaseTests : OperatorUseCaseTestBase
{
    private readonly UpdateOperatorSettingsUseCase _sut;

    public UpdateOperatorSettingsUseCaseTests()
    {
        _sut = new UpdateOperatorSettingsUseCase(Uow.Object);
    }

    private static UpdateOperatorSettingsRequest ValidRequest() =>
        new UpdateOperatorSettingsRequest
        {
            Name         = "NewName",
            Country      = "DE",
            OperatorType = EOperatorType.Government
        };

    [Fact]
    public async Task ExecuteAsync_WhenOperatorNotFound_ThrowsException()
    {
        OperatorRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Operator?)null);

        Func<Task> act = () => _sut.ExecuteAsync(Guid.NewGuid(), ValidRequest());

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorNotFound_DoesNotCommit()
    {
        OperatorRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Operator?)null);

        try { await _sut.ExecuteAsync(Guid.NewGuid(), ValidRequest()); } catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesCountryCode()
    {
        var op      = MakeOperator();
        var request = ValidRequest();
        OperatorRepo
            .Setup(r => r.FindAsync(op.Id, It.IsAny<Guid>()))
            .ReturnsAsync(op);
        OperatorRepo
            .Setup(r => r.UpdateAsync(op, It.IsAny<Guid>()))
            .ReturnsAsync(op);

        await _sut.ExecuteAsync(op.Id, request);

        op.CountryCode.Should().Be(request.Country);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesOperatorType()
    {
        var op      = MakeOperator();
        var request = ValidRequest();
        OperatorRepo
            .Setup(r => r.FindAsync(op.Id, It.IsAny<Guid>()))
            .ReturnsAsync(op);
        OperatorRepo
            .Setup(r => r.UpdateAsync(op, It.IsAny<Guid>()))
            .ReturnsAsync(op);

        await _sut.ExecuteAsync(op.Id, request);

        op.OperatorType.Should().Be(request.OperatorType);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CallsUpdateAsync()
    {
        var op = MakeOperator();
        OperatorRepo
            .Setup(r => r.FindAsync(op.Id, It.IsAny<Guid>()))
            .ReturnsAsync(op);
        OperatorRepo
            .Setup(r => r.UpdateAsync(op, It.IsAny<Guid>()))
            .ReturnsAsync(op);

        await _sut.ExecuteAsync(op.Id, ValidRequest());

        OperatorRepo.Verify(
            r => r.UpdateAsync(op, It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CommitsUnitOfWork()
    {
        var op = MakeOperator();
        OperatorRepo
            .Setup(r => r.FindAsync(op.Id, It.IsAny<Guid>()))
            .ReturnsAsync(op);
        OperatorRepo
            .Setup(r => r.UpdateAsync(op, It.IsAny<Guid>()))
            .ReturnsAsync(op);

        await _sut.ExecuteAsync(op.Id, ValidRequest());

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}