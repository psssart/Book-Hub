using WebApp.Infrastructure.Data.SeedDTO;

namespace WebApp.Infrastructure.Data;

/// <summary>
/// Represents configuration settings for database seeding.
/// </summary>
public class SeedSettings
{
    /// <summary>
    /// List of users to seed, including roles and passwords.
    /// </summary>
    public List<UserSeedDto> Users { get; set; } = new();
    /// <summary>
    /// Number of publishers to seed (from a predefined JSON list).
    /// </summary>
    public int Publishers { get; set; }
    /// <summary>
    /// Number of authors to seed (from predefined JSON list).
    /// </summary>
    public int Authors { get; set; }
    /// <summary>
    /// Whether to seed a predefined list of genres.
    /// </summary>
    public bool Genres { get; set; }
    /// <summary>
    /// Number of warehouses to seed (from predefined JSON list).
    /// </summary>
    public int Warehouses { get; set; }
    /// <summary>
    /// Number of books to seed (from a predefined JSON list).
    /// </summary>
    public int Books { get; set; }
    /// <summary>
    /// Whether to seed a predefined list of discussions with topics and messages.
    /// </summary>
    public bool Discussions { get; set; }
}