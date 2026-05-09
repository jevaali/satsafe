namespace App.Tests.Integration.Helpers;

public static class TestUsers
{
    public const string Password = "Test.pass1";

    // UserA is seeded as "Operator" role — can use all operator endpoints
    public static readonly Guid UserAId = new("11111111-1111-1111-1111-111111111111");
    public const string UserAEmail = "user-a@test.ee";

    // UserB is seeded as "OperatorHelper" role — read-only operator endpoints only
    public static readonly Guid UserBId = new("22222222-2222-2222-2222-222222222222");
    public const string UserBEmail = "user-b@test.ee";

    public static readonly Guid OperatorId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static readonly Guid TestSatelliteId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public const int TestSatelliteNoradId = 12345;
    public const string TestSatelliteName = "test-satellite";

    public static readonly Guid TestAlertId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public static readonly Guid TestConjunctionId = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
}