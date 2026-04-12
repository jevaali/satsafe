namespace App.Application.DTO.v1;

public class RestApiErrorResponse
{
    public System.Net.HttpStatusCode Status { get; init; }
    public string Error { get; init; } = default!;
}
