namespace App.Application.DTO;

public class MainListDTO
{
    public List<MainListSatDTO> Satellites { get; set; } = [];
    public List<MainListDebrisDTO> Debris { get; set; } = [];
    
}

public class MainListSatDTO
{
    public string Name { get; set; } = default!;
    public string Operator { get; set; } = default!;
}

public class MainListDebrisDTO
{
    public int NoradId { get; set; } = default!;
    public float SizeEstimate { get; set; } = default!;
}