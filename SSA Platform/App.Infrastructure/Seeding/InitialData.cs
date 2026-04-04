namespace App.Infrastructure.Seeding;

public static class InitialData
{
    public static readonly string[] Roles = [
        "Operator",
        "User",
        "OperatorHelper",
        "Admin"
    ];
    
    public static readonly (string email, string password, string[] roles)[] Users = [
        ("admin@ssa.com", "Admin@123", ["Admin"]),
        ("user@ssa.com", "User@123", ["User"]),
        ("operator@ssa.com", "Operator@123", ["Operator"]) ];
}
