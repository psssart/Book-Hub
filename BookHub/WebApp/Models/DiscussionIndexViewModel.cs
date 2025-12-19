namespace WebApp.Models;

public class DiscussionIndexViewModel
{
    // Core data - list of discussions with computed metrics
    public List<DiscussionCardData> Discussions { get; set; } = new();

    // Filter options (distinct values for checkboxes)
    public List<App.Domain.Entities.Author> AvailableAuthors { get; set; } = new();
    public List<App.Domain.Entities.Genre> AvailableGenres { get; set; } = new();
    public List<App.Domain.Entities.Book> AvailableBooks { get; set; } = new();

    // Current filter state (to preserve selection after AJAX)
    public string? SearchInput { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

// Discussion card data with computed metrics
public class DiscussionCardData
{
    public Guid Id { get; set; }
    public string Tittle { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreationTime { get; set; }
    public byte[]? ImageData { get; set; }

    // Creator info
    public string CreatorUsername { get; set; } = default!;
    public Guid CreatorId { get; set; }

    // Optional associations
    public string? BookTittle { get; set; }
    public Guid? BookId { get; set; }
    public string? GenreName { get; set; }
    public Guid? GenreId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? AuthorId { get; set; }

    // Computed metrics for sorting/display
    public int ParticipantsCount { get; set; }  // Unique users (topic creators + message authors)
    public int TopicsCount { get; set; }
    public int MessagesCount { get; set; }
    public DateTime? LastActivityTime { get; set; }  // Most recent topic or message
}
