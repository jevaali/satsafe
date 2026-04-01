using App.Domain.Entities.Identity;
using Base.Domain;

namespace App.Domain.Entities;

public class UserOperator : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    
    public Guid OperatorId { get; set; }
    public Operator? Operator { get; set; }
}
