namespace WebApp.Infrastructure.Data.SeedDTO;

/// <summary>
/// Warehouse DTO for seeding
/// </summary>
public class WarehouseSeedDto
{
    /// <summary>
    /// Warehouse name
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// Warehouse GPS X coordinate
    /// </summary>
    public double GpsX { get; set; }
    /// <summary>
    /// Warehouse GPS Y coordinate
    /// </summary>
    public double GpsY { get; set; }
}