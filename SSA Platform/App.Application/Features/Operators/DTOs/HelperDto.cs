namespace App.Application.Features.Operators.DTOs;

public class HelperDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials
    {
        get
        {
            var i = $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper().Trim();
            return string.IsNullOrWhiteSpace(i)
                ? (Email.FirstOrDefault() is char c ? c.ToString().ToUpper() : "?")
                : i;
        }
    }
}
