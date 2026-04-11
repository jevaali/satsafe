namespace App.Application.DTO;

public class OperatorActionLogDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
