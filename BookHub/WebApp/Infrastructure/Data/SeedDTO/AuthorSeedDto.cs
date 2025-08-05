namespace WebApp.Infrastructure.Data.SeedDTO;

/// <summary>
/// Author DTO for seeding
/// </summary>
public class AuthorSeedDto
{
    /// <summary>
    /// Author full name
    /// </summary>
    public string FullName { get; set; } = null!;
    /// <summary>
    /// Author biography
    /// </summary>
    public string Biography { get; set; } = null!;
    /// <summary>
    /// Author birth Year
    /// </summary>
    public int BirthYear { get; set; }
}