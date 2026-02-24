using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class UserOperator : BaseEntity
{
    // Foreign keys
    public Guid UserId { get; set; }
    public Guid OperatorId { get; set; }

    // Navigation properties
    public AppUser? User { get; set; }
    public Operator? Operator { get; set; }
}
