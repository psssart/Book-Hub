namespace WebApp.Infrastructure.Data.SeedDTO;

/// <summary>
/// Book DTO for seeding
/// </summary>
public class BookSeedDto
{
    /// <summary>
    /// Book title
    /// </summary>
    public string Title { get; set; } = null!;
    /// <summary>
    /// List of Book author names
    /// </summary>
    public List<string> AuthorNames { get; set; } = new();
    /// <summary>
    /// Book description
    /// </summary>
    public string Description { get; set; } = null!;
    /// <summary>
    /// Book release year
    /// </summary>
    public int ReleaseYear { get; set; }
    /// <summary>
    /// Book publisher
    /// </summary>
    public string Publisher { get; set; } = null!;
    /// <summary>
    /// List of Book genres
    /// </summary>
    public List<string> Genres { get; set; } = new();
    /// <summary>
    /// Book price
    /// </summary>
    public float Price { get; set; }
}