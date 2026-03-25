using System.ComponentModel.DataAnnotations;
using App.Domain.Enums;
using Base.Domain;

namespace App.Domain;

public class Operator : BaseEntityWithMeta
{
    
    [MaxLength(124)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(124)]
    public string Country { get; set; } = string.Empty;
    
    [MaxLength(124)]
    public string? OperatorTypeString { get; set; } = string.Empty;
    
    public EOperatorType? OperatorType { get; set; }

    public ICollection<Satellite> Satellites { get; set; } = [];
    public ICollection<UserOperator> UserOperators { get; set; } = [];
}
