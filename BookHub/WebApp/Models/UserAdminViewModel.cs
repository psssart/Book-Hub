namespace WebApp.Models;

public class UserAdminViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public byte[]? AvatarImageData { get; set; }
    public List<string> Roles { get; set; } = new();

    // Stats
    public int PurchasedBooksCount { get; set; }
    public int RatingsCount { get; set; }
    public int DiscussionsCount { get; set; }
    public int TopicsCount { get; set; }
    public int MessagesCount { get; set; }
}

public class UserEditViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public byte[]? AvatarImageData { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();
}
