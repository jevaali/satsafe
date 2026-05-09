using App.Domain.Entities;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Entities;

public class RiskAssessmentTests
{
    private static RiskAssessment MakeAssessment(double score = 0.5) =>
        new RiskAssessment
        {
            RiskScore      = score,
            ModelVersion   = "v1.0",
            CalculatedAt   = DateTime.UtcNow,
            Satellite      = null!
        };
    
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void UpdateScore_WithValidScore_UpdatesRiskScore(double score)
    {
        var assessment = MakeAssessment();

        assessment.UpdateScore(score);

        assessment.RiskScore.Should().Be(score);
    }

    [Fact]
    public void UpdateScore_WithValidScore_UpdatesCalculatedAt()
    {
        var assessment   = MakeAssessment();
        var beforeUpdate = DateTime.UtcNow;

        assessment.UpdateScore(0.8);

        assessment.CalculatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Theory]
    [InlineData(-0.001)]
    [InlineData(1.001)]
    [InlineData(-50)]
    public void UpdateScore_WithOutOfRangeScore_ThrowsArgumentException(double score)
    {
        var assessment = MakeAssessment();

        Action act = () => assessment.UpdateScore(score);

        act.Should().Throw<ArgumentException>();
    }
    
    public static IEnumerable<object[]> RiskLevelCases()
    {
        yield return new object[] { 0.71,  "High"   };
        yield return new object[] { 1.0,   "High"   };
        yield return new object[] { 0.41,  "Medium" };
        yield return new object[] { 0.7,   "Medium" };
        yield return new object[] { 0.0,   "Low"    };
        yield return new object[] { 0.4,   "Low"    };
    }

    [Theory]
    [MemberData(nameof(RiskLevelCases))]
    public void RiskLevel_ReturnsCorrectLabel(double score, string expectedLevel)
    {
        var assessment = MakeAssessment(score);

        assessment.RiskLevel.Should().Be(expectedLevel);
    }
}