using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Notifications.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Notifications;

public class SaveNotificationsUseCaseTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork>                      _uow;
    private readonly Mock<INotificationSettingRepository>   _settingsRepo;
    private readonly SaveNotificationsUseCase               _sut;

    public SaveNotificationsUseCaseTests()
    {
        _uow          = new Mock<IUnitOfWork>();
        _settingsRepo = new Mock<INotificationSettingRepository>();
        _uow.Setup(u => u.NotificationSettings).Returns(_settingsRepo.Object);
        _sut = new SaveNotificationsUseCase(_uow.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SaveNotificationsRequest ValidRequest() => new SaveNotificationsRequest
    {
        ConjunctionThresholdKm = 50.0,
        AlertThresholdKm       = 5.0,
        EmailEnabled           = true,
        SmsEnabled             = false
    };

    private static NotificationSetting ExistingSetting(Guid userId) => new NotificationSetting
    {
        Id                     = Guid.NewGuid(),
        AppUserId              = userId,
        ConjunctionThresholdKm = 100.0,
        AlertThresholdKm       = 10.0,
        EmailEnabled           = false,
        SmsEnabled             = false,
        CreatedAt              = DateTime.UtcNow.AddDays(-1),
        CreatedBy              = userId.ToString()
    };

    private void SetupNoExistingSetting(Guid userId) =>
        _settingsRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((NotificationSetting?)null);

    private void SetupExistingSetting(Guid userId, NotificationSetting setting) =>
        _settingsRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(setting);

    // ── Create path (no existing setting) ────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenNoExistingSetting_AddsNewSetting()
    {
        var userId = Guid.NewGuid();
        SetupNoExistingSetting(userId);

        await _sut.ExecuteAsync(userId, ValidRequest());

        _settingsRepo.Verify(
            r => r.Add(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExistingSetting_DoesNotCallUpdate()
    {
        var userId = Guid.NewGuid();
        SetupNoExistingSetting(userId);

        await _sut.ExecuteAsync(userId, ValidRequest());

        _settingsRepo.Verify(
            r => r.UpdateAsync(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExistingSetting_NewSettingHasCorrectUserId()
    {
        var userId = Guid.NewGuid();
        SetupNoExistingSetting(userId);

        NotificationSetting? added = null;
        _settingsRepo
            .Setup(r => r.Add(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()))
            .Callback<NotificationSetting, Guid>((s, _) => added = s);

        await _sut.ExecuteAsync(userId, ValidRequest());

        added!.AppUserId.Should().Be(userId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExistingSetting_NewSettingMapsRequestValues()
    {
        var userId  = Guid.NewGuid();
        var request = ValidRequest();
        SetupNoExistingSetting(userId);

        NotificationSetting? added = null;
        _settingsRepo
            .Setup(r => r.Add(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()))
            .Callback<NotificationSetting, Guid>((s, _) => added = s);

        await _sut.ExecuteAsync(userId, request);

        added!.ConjunctionThresholdKm.Should().Be(request.ConjunctionThresholdKm);
        added.AlertThresholdKm.Should().Be(request.AlertThresholdKm);
        added.EmailEnabled.Should().Be(request.EmailEnabled);
        added.SmsEnabled.Should().Be(request.SmsEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExistingSetting_CreatedByIsUserId()
    {
        var userId = Guid.NewGuid();
        SetupNoExistingSetting(userId);

        NotificationSetting? added = null;
        _settingsRepo
            .Setup(r => r.Add(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()))
            .Callback<NotificationSetting, Guid>((s, _) => added = s);

        await _sut.ExecuteAsync(userId, ValidRequest());

        added!.CreatedBy.Should().Be(userId.ToString());
    }

    // ── Update path (existing setting found) ──────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenExistingSetting_CallsUpdateAsync()
    {
        var userId  = Guid.NewGuid();
        var setting = ExistingSetting(userId);
        SetupExistingSetting(userId, setting);
        _settingsRepo
            .Setup(r => r.UpdateAsync(setting, It.IsAny<Guid>()))
            .ReturnsAsync(setting);

        await _sut.ExecuteAsync(userId, ValidRequest());

        _settingsRepo.Verify(
            r => r.UpdateAsync(setting, It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExistingSetting_DoesNotAddNewSetting()
    {
        var userId  = Guid.NewGuid();
        var setting = ExistingSetting(userId);
        SetupExistingSetting(userId, setting);
        _settingsRepo
            .Setup(r => r.UpdateAsync(setting, It.IsAny<Guid>()))
            .ReturnsAsync(setting);

        await _sut.ExecuteAsync(userId, ValidRequest());

        _settingsRepo.Verify(
            r => r.Add(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExistingSetting_UpdatesAllFieldsFromRequest()
    {
        var userId  = Guid.NewGuid();
        var setting = ExistingSetting(userId);
        var request = ValidRequest();
        SetupExistingSetting(userId, setting);
        _settingsRepo
            .Setup(r => r.UpdateAsync(setting, It.IsAny<Guid>()))
            .ReturnsAsync(setting);

        await _sut.ExecuteAsync(userId, request);

        setting.ConjunctionThresholdKm.Should().Be(request.ConjunctionThresholdKm);
        setting.AlertThresholdKm.Should().Be(request.AlertThresholdKm);
        setting.EmailEnabled.Should().Be(request.EmailEnabled);
        setting.SmsEnabled.Should().Be(request.SmsEnabled);
    }

    // ── Commit always called ──────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_OnCreatePath_CommitsUnitOfWork()
    {
        var userId = Guid.NewGuid();
        SetupNoExistingSetting(userId);

        await _sut.ExecuteAsync(userId, ValidRequest());

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_OnUpdatePath_CommitsUnitOfWork()
    {
        var userId  = Guid.NewGuid();
        var setting = ExistingSetting(userId);
        SetupExistingSetting(userId, setting);
        _settingsRepo
            .Setup(r => r.UpdateAsync(setting, It.IsAny<Guid>()))
            .ReturnsAsync(setting);

        await _sut.ExecuteAsync(userId, ValidRequest());

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    // ── Theory: various threshold combinations ────────────────────────────────

    public static IEnumerable<object[]> ThresholdCombinations()
    {
        yield return new object[] { 10.0,  1.0  };
        yield return new object[] { 50.0,  5.0  };
        yield return new object[] { 100.0, 10.0 };
    }

    [Theory]
    [MemberData(nameof(ThresholdCombinations))]
    public async Task ExecuteAsync_WhenCreating_StoresCorrectThresholds(
        double conjunctionKm, double alertKm)
    {
        var userId  = Guid.NewGuid();
        var request = new SaveNotificationsRequest
        {
            ConjunctionThresholdKm = conjunctionKm,
            AlertThresholdKm       = alertKm,
            EmailEnabled           = false,
            SmsEnabled             = false
        };
        SetupNoExistingSetting(userId);

        NotificationSetting? added = null;
        _settingsRepo
            .Setup(r => r.Add(It.IsAny<NotificationSetting>(), It.IsAny<Guid>()))
            .Callback<NotificationSetting, Guid>((s, _) => added = s);

        await _sut.ExecuteAsync(userId, request);

        added!.ConjunctionThresholdKm.Should().Be(conjunctionKm);
        added.AlertThresholdKm.Should().Be(alertKm);
    }
}