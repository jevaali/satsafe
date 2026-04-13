using App.Application.Features.Satellites;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;

namespace App.Infrastructure.ExternalServices;

public class CelesTrakService : ICelesTrakService
{
    private readonly HttpClient _http;

    public CelesTrakService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CelesTrakTleDto?> FetchTleByNoradIdAsync(int noradId)
    {
        try
        {
            var text  = await _http.GetStringAsync($"/NORAD/elements/gp.php?CATNR={noradId}&FORMAT=TLE");
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (lines.Length < 3) return null;

            return TleParser.Parse(lines[1], lines[2], lines[0]);
        }
        catch
        {
            return null;
        }
    }
}
