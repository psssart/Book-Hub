namespace WebApp.Infrastructure.Data.SeedDTO;

/// <summary>
/// Genre DTO for seeding
/// </summary>
public class GenreSeedDto
{
    /// <summary>
    /// Genre name
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// Genre description
    /// </summary>
    public string Description { get; set; } = null!;
    /// <summary>
    /// Is it a main/secondary genre?
    /// </summary>
    public bool IsMainGenre { get; set; }
}