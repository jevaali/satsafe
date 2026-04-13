using App.Application.Features.Satellites.DTOs;

namespace App.Application.Interfaces;

public interface ICelesTrakService
{
    Task<CelesTrakTleDto?> FetchTleByNoradIdAsync(int noradId);
}
