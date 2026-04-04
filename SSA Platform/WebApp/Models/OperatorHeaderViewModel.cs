namespace WebApp.Models;

public class OperatorHeaderViewModel
{
    public List<SatelliteItem> Satellites { get; set; } = new();
}

public class SatelliteItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}