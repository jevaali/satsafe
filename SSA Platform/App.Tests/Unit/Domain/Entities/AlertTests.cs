using App.Domain.Entities;
using App.Domain.Entities.Enums;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Entities;

public class AlertTests
{
    private static Alert MakeAlert(bool isAcknowledged = false) =>
        new Alert
        {
            UserId             = Guid.NewGuid(),
            ConjunctionEventId = Guid.NewGuid(),
            Severity           = EAlertSeverity.High,
            IsAcknowledged     = isAcknowledged,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = "system"
        };

    [Fact]
    public void Acknowledge_WhenNotYetAcknowledged_SetsIsAcknowledgedToTrue()
    {
        var alert = MakeAlert(isAcknowledged: false);

        alert.Acknowledge();

        alert.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_RemainsTrue()
    {
        var alert = MakeAlert(isAcknowledged: true);
        
        alert.Acknowledge();

        alert.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_IsIdempotent()
    {
        var alert = MakeAlert(isAcknowledged: false);
        alert.Acknowledge();
        
        Action act = () => alert.Acknowledge();

        act.Should().NotThrow();
    }
}