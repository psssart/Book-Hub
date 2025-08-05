namespace WebApp.Infrastructure.Data.SeedDTO;

/// <summary>
/// User DTO for seeding
/// </summary>
public class UserSeedDto
{
    /// <summary>
    /// User email
    /// </summary>
    public string Email { get; set; } = null!;
    /// <summary>
    /// User first name
    /// </summary>
    public string FirstName { get; set; } = null!;
    /// <summary>
    /// User last name
    /// </summary>
    public string LastName { get; set; } = null!;
    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; } = null!;
    /// <summary>
    /// List of user roles
    /// </summary>
    public List<string> Roles { get; set; } = new();
}