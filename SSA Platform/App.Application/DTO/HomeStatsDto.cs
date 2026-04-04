namespace App.Application.DTO;

public class HomeStatsDto
{
    public int TotalSpaceObjects { get; set; }
    public int ActiveSatellites { get; set; }
    public int DebrisObjects { get; set; }

    public List<HomeStatsSatteliteDto> Satellites { get; set; } = [];
}
public class HomeStatsSatteliteDto
{
    public string Name { get; set; } = default!;
    public string Operator { get; set; } = default!;
}