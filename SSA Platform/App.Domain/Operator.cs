using Base.Domain;

namespace App.Domain;

public class Operator : BaseEntityWithMeta
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public ICollection<Satellite> Satellites { get; set; } = [];
    public ICollection<UserOperator> UserOperators { get; set; } = [];
}
