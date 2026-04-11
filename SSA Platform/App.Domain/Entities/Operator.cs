using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using Base.Domain;

namespace App.Domain.Entities;

public class Operator : BaseEntityWithMeta
{
    
    [MaxLength(124)]
    public LangStr Name { get; set; } = string.Empty;
    
    [MaxLength(124)]
    public string CountryCode { get; set; } = string.Empty;
    
    [MaxLength(124)]
    public LangStr? OperatorTypeString { get; set; } = string.Empty;
    
    public EOperatorType? OperatorType { get; set; }

    public ICollection<Satellite> Satellites { get; set; } = [];
    public ICollection<UserOperator> UserOperators { get; set; } = [];
}
