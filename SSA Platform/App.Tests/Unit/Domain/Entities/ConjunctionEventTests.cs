using App.Domain.Entities;
using App.Domain.Entities.Enums;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Entities;

public class ConjunctionEventTests
{
    private static ConjunctionEvent MakeEvent(
        double missDistance = 5000,
        double threshold = 10_000,
        DateTime? tca = null) =>
        new ConjunctionEvent(
            primaryObjectId:       Guid.NewGuid(),
            secondaryObjectId:     Guid.NewGuid(),
            timeOfClosestApproach: tca ?? DateTime.UtcNow.AddHours(48),
            missDistance:          missDistance,
            threshold:             threshold);
    
    [Fact]
    public void Constructor_WithValidArguments_SetsStatusToPending()
    {
        var evt = MakeEvent();

        evt.Status.Should().Be(EConjunctionEventStatus.Pending);
    }

    [Fact]
    public void Constructor_WithValidArguments_SetsCreatedByToSystem()
    {
        var evt = MakeEvent();

        evt.CreatedBy.Should().Be("system");
    }
    
    [Fact]
    public void IsThreat_WhenMissDistanceBelowThreshold_ReturnsTrue()
    {
        var evt = MakeEvent(missDistance: 500, threshold: 1000);

        evt.IsThreat.Should().BeTrue();
    }

    [Fact]
    public void IsThreat_WhenMissDistanceAboveThreshold_ReturnsFalse()
    {
        var evt = MakeEvent(missDistance: 2000, threshold: 1000);

        evt.IsThreat.Should().BeFalse();
    }
    
    
    [Fact]
    public void UpdateMissDistance_WithPositiveValue_UpdatesMissDistance()
    {
        var evt = MakeEvent(missDistance: 5000);

        evt.UpdateMissDistance(3000);

        evt.MissDistance.Should().Be(3000);
    }

    [Fact]
    public void UpdateMissDistance_WithZero_UpdatesMissDistanceToZero()
    {
        var evt = MakeEvent(missDistance: 5000);

        evt.UpdateMissDistance(0);

        evt.MissDistance.Should().Be(0);
    }

    [Fact]
    public void UpdateMissDistance_WithNegativeValue_ThrowsArgumentException()
    {
        var evt = MakeEvent();

        Action act = () => evt.UpdateMissDistance(-1);

        act.Should().Throw<ArgumentException>();
    }
    
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void UpdateCollisionProbability_WithValidValue_UpdatesProbability(double probability)
    {
        var evt = MakeEvent();

        evt.UpdateCollisionProbability(probability);

        evt.CollisionProbability.Should().Be(probability);
    }

    [Theory]
    [InlineData(-0.001)]
    [InlineData(1.001)]
    [InlineData(-100)]
    public void UpdateCollisionProbability_WithOutOfRangeValue_ThrowsArgumentException(double probability)
    {
        var evt = MakeEvent();

        Action act = () => evt.UpdateCollisionProbability(probability);

        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void IsHighRisk_WhenCollisionProbabilityAboveThreshold_ReturnsTrue()
    {
        var evt = MakeEvent();
        evt.UpdateCollisionProbability(0.02);

        evt.IsHighRisk().Should().BeTrue();
    }

    [Fact]
    public void IsHighRisk_WhenMissDistanceBelow1000_ReturnsTrue()
    {
        var evt = MakeEvent(missDistance: 999);

        evt.IsHighRisk().Should().BeTrue();
    }

    [Fact]
    public void IsHighRisk_WhenBothBelowThresholds_ReturnsFalse()
    {
        var evt = MakeEvent(missDistance: 2000);
        evt.UpdateCollisionProbability(0.005);

        evt.IsHighRisk().Should().BeFalse();
    }
    
    [Fact]
    public void RefreshStatus_WhenTcaInPast_SetsStatusToAlreadyOccurred()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(-1));

        evt.RefreshStatus();

        evt.Status.Should().Be(EConjunctionEventStatus.AlreadyOccurred);
    }

    [Fact]
    public void RefreshStatus_WhenTcaWithin24Hours_SetsStatusToImminent()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(12));

        evt.RefreshStatus();

        evt.Status.Should().Be(EConjunctionEventStatus.Imminent);
    }

    [Fact]
    public void RefreshStatus_WhenTcaWithin72Hours_SetsStatusToPotential()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(48));

        evt.RefreshStatus();

        evt.Status.Should().Be(EConjunctionEventStatus.Potential);
    }

    [Fact]
    public void RefreshStatus_WhenTcaBeyond72Hours_SetsStatusToPending()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(100));

        evt.RefreshStatus();

        evt.Status.Should().Be(EConjunctionEventStatus.Pending);
    }

    [Fact]
    public void RefreshStatus_WhenAlreadyResolved_DoesNotChangeStatus()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(48));
        evt.MarkAsResolved();

        evt.RefreshStatus();

        evt.Status.Should().Be(EConjunctionEventStatus.Resolved);
    }
    
    [Fact]
    public void MarkAsResolved_WhenEventIsPending_SetsStatusToResolved()
    {
        var evt = MakeEvent();

        evt.MarkAsResolved();

        evt.Status.Should().Be(EConjunctionEventStatus.Resolved);
    }

    [Fact]
    public void MarkAsResolved_WhenEventAlreadyOccurred_ThrowsInvalidOperationException()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(-1));
        evt.RefreshStatus();

        Action act = () => evt.MarkAsResolved();

        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void AcknowledgePrimarySide_WithHigherState_UpdatesAcknowledgement()
    {
        var evt = MakeEvent();

        evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Confirmed);

        evt.PrimaryAcknowledgement.Should().Be(EOperatorAcknowledgement.Confirmed);
    }

    [Fact]
    public void AcknowledgePrimarySide_WithSameOrLowerState_ThrowsInvalidOperationException()
    {
        var evt = MakeEvent();
        evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Confirmed);

        Action act = () => evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Confirmed);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AutoResolve_WhenBothSidesResolvedAndNotPast_SetsStatusToResolved()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(48));

        evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Confirmed);
        evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Resolved);
        evt.AcknowledgeSecondarySide(EOperatorAcknowledgement.Confirmed);
        evt.AcknowledgeSecondarySide(EOperatorAcknowledgement.Resolved);

        evt.Status.Should().Be(EConjunctionEventStatus.Resolved);
    }

    [Fact]
    public void AutoResolve_WhenBothSidesResolvedButAlreadyOccurred_DoesNotResolve()
    {
        var evt = MakeEvent(tca: DateTime.UtcNow.AddHours(-1));
        evt.RefreshStatus();

        evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Confirmed);
        evt.AcknowledgePrimarySide(EOperatorAcknowledgement.Resolved);
        evt.AcknowledgeSecondarySide(EOperatorAcknowledgement.Confirmed);
        evt.AcknowledgeSecondarySide(EOperatorAcknowledgement.Resolved);

        evt.Status.Should().Be(EConjunctionEventStatus.AlreadyOccurred);
    }


    [Fact]
    public void GetOrderedPair_Always_ReturnsMinThenMax()
    {
        var low  = new Guid("00000000-0000-0000-0000-000000000001");
        var high = new Guid("00000000-0000-0000-0000-000000000002");

        var evt = new ConjunctionEvent(high, low, DateTime.UtcNow.AddHours(48), 5000, 10_000);

        var (min, max) = evt.GetOrderedPair();

        min.Should().Be(low);
        max.Should().Be(high);
    }

    [Fact]
    public void GetOrderedPair_IsSymmetric_BothOrderingsProduceSameResult()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        var evt1 = new ConjunctionEvent(a, b, DateTime.UtcNow.AddHours(48), 5000, 10_000);
        var evt2 = new ConjunctionEvent(b, a, DateTime.UtcNow.AddHours(48), 5000, 10_000);

        evt1.GetOrderedPair().Should().Be(evt2.GetOrderedPair());
    }
    
    [Fact]
    public void CreateAlert_WhenMissDistanceBelowThreshold_ReturnsAlert()
    {
        var evt    = MakeEvent(missDistance: 500);
        var userId = Guid.NewGuid();

        var alert = evt.CreateAlert(userId, alertThresholdKm: 10);

        alert.Should().NotBeNull();
        alert!.UserId.Should().Be(userId);
    }

    [Fact]
    public void CreateAlert_WhenMissDistanceAboveThreshold_ReturnsNull()
    {
        var evt = MakeEvent(missDistance: 20_000);

        var alert = evt.CreateAlert(Guid.NewGuid(), alertThresholdKm: 10);

        alert.Should().BeNull();
    }

    [Fact]
    public void CreateAlert_WhenMissDistanceWithinOneFifthOfThreshold_SetsSeverityToCritical()
    {
        // threshold = 10 km = 10_000 m; critical when <= 2 km = 2_000 m
        var evt = MakeEvent(missDistance: 1000);

        var alert = evt.CreateAlert(Guid.NewGuid(), alertThresholdKm: 10);

        alert!.Severity.Should().Be(EAlertSeverity.Critical);
    }

    [Fact]
    public void CreateAlert_WhenMissDistanceBetweenOneFifthAndFullThreshold_SetsSeverityToHigh()
    {
        // threshold = 10 km; high when between 2 km and 10 km
        var evt = MakeEvent(missDistance: 5000);

        var alert = evt.CreateAlert(Guid.NewGuid(), alertThresholdKm: 10);

        alert!.Severity.Should().Be(EAlertSeverity.High);
    }
    
    public static IEnumerable<object[]> GetMissDistancesBelowThreshold()
    {
        yield return new object[] { 100.0 };
        yield return new object[] { 500.0 };
        yield return new object[] { 999.0 };
    }

    [Theory]
    [MemberData(nameof(GetMissDistancesBelowThreshold))]
    public void CreateAlert_WithVariousBelowThresholdDistances_AlwaysReturnsAlert(double missDistance)
    {
        var evt = MakeEvent(missDistance: missDistance);

        var alert = evt.CreateAlert(Guid.NewGuid(), alertThresholdKm: 10);

        alert.Should().NotBeNull();
    }
}