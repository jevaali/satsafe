using App.Application.Features.Users.DTOs;
using App.Application.Features.Users.UseCases;
using App.Domain.Entities.Identity;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace App.Tests.Unit.UseCases.Users;

public class RegisterUserUseCaseTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly RegisterUserUseCase        _sut;

    public RegisterUserUseCaseTests()
    {
        // UserManager has no parameterless constructor — Moq requires us to
        // pass a real IUserStore mock and nulls for the rest of the optional args.
        _userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        _sut = new RegisterUserUseCase(_userManager.Object);
    }

    // ── Helpers ───────────────────────────────────────────────name────────────

    private static RegisterUserRequest ValidRequest() => new RegisterUserRequest
    {
        Email     = "test@example.com",
        Password  = "Password123!",
        FirstName = "John",
        LastName  = "Doe"
    };

    private void SetupEmailNotTaken() =>
        _userManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

    private void SetupEmailTaken() =>
        _userManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new AppUser());

    private void SetupCreateSuccess() =>
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

    private void SetupCreateFailure(string error = "Password too weak") =>
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = error }));

    private void SetupAddRoleSuccess() =>
        _userManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

    private void SetupAddRoleFailure() =>
        _userManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role error" }));

    // ── Email already registered ──────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenEmailAlreadyRegistered_ReturnsFailure()
    {
        SetupEmailTaken();

        var (success, _) = await _sut.Execute(ValidRequest());

        success.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenEmailAlreadyRegistered_ReturnsEmailInUseError()
    {
        SetupEmailTaken();

        var (_, error) = await _sut.Execute(ValidRequest());

        error.Should().Contain("already registered");
    }

    [Fact]
    public async Task Execute_WhenEmailAlreadyRegistered_DoesNotCallCreate()
    {
        SetupEmailTaken();

        await _sut.Execute(ValidRequest());

        _userManager.Verify(
            m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── CreateAsync fails ─────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenCreateFails_ReturnsFailure()
    {
        SetupEmailNotTaken();
        SetupCreateFailure("Password too weak");

        var (success, _) = await _sut.Execute(ValidRequest());

        success.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenCreateFails_ErrorContainsIdentityDescription()
    {
        SetupEmailNotTaken();
        SetupCreateFailure("Password too weak");

        var (_, error) = await _sut.Execute(ValidRequest());

        error.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task Execute_WhenCreateFails_DoesNotAddRole()
    {
        SetupEmailNotTaken();
        SetupCreateFailure();

        await _sut.Execute(ValidRequest());

        _userManager.Verify(
            m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── AddToRoleAsync fails ──────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenAddRoleFails_ReturnsFailure()
    {
        SetupEmailNotTaken();
        SetupCreateSuccess();
        SetupAddRoleFailure();
        _userManager
            .Setup(m => m.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var (success, _) = await _sut.Execute(ValidRequest());

        success.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenAddRoleFails_DeletesCreatedUser()
    {
        SetupEmailNotTaken();
        SetupCreateSuccess();
        SetupAddRoleFailure();
        _userManager
            .Setup(m => m.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Execute(ValidRequest());

        _userManager.Verify(
            m => m.DeleteAsync(It.IsAny<AppUser>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenAddRoleFails_ErrorMentionsRole()
    {
        SetupEmailNotTaken();
        SetupCreateSuccess();
        SetupAddRoleFailure();
        _userManager
            .Setup(m => m.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var (_, error) = await _sut.Execute(ValidRequest());

        error.Should().Contain("role");
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WithValidRequest_ReturnsSuccess()
    {
        SetupEmailNotTaken();
        SetupCreateSuccess();
        SetupAddRoleSuccess();

        var (success, error) = await _sut.Execute(ValidRequest());

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task Execute_WithValidRequest_CreatesUserWithCorrectEmail()
    {
        SetupEmailNotTaken();
        SetupAddRoleSuccess();

        AppUser? created = null;
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .Callback<AppUser, string>((u, _) => created = u)
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Execute(ValidRequest());

        created!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Execute_WithValidRequest_AssignsUserRole()
    {
        SetupEmailNotTaken();
        SetupCreateSuccess();
        SetupAddRoleSuccess();

        await _sut.Execute(ValidRequest());

        _userManager.Verify(
            m => m.AddToRoleAsync(It.IsAny<AppUser>(), "User"),
            Times.Once);
    }

    // ── Theory: multiple invalid create scenarios ─────────────────────────────

    public static IEnumerable<object[]> CreateFailureMessages()
    {
        yield return new object[] { "Password too weak" };
        yield return new object[] { "Invalid email format" };
        yield return new object[] { "Username already taken" };
    }

    [Theory]
    [MemberData(nameof(CreateFailureMessages))]
    public async Task Execute_WhenCreateFailsWithVariousErrors_ErrorContainsDescription(
        string errorDescription)
    {
        SetupEmailNotTaken();
        SetupCreateFailure(errorDescription);

        var (_, error) = await _sut.Execute(ValidRequest());

        error.Should().Contain(errorDescription);
    }
}