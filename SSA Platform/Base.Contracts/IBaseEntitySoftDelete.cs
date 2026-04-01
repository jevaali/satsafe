using System.ComponentModel.DataAnnotations;

namespace Base.Contracts;

public interface IBaseEntitySoftDelete
{
    [MaxLength(128)]
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
