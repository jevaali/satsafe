using Base.Domain;

namespace App.Domain;

public class Operator : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    // Navigation properties
    public List<Satellite> Satellites { get; set; } = new();
    public List<UserOperator> UserOperators { get; set; } = new();
}
