namespace Base.Contracts;

public interface IEntityMetaDelete
{
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}