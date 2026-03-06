namespace WebApp.Infrastructure.Data.SeedDTO;

public class DiscussionSeedDto
{
    public string Tittle { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? BookTitle { get; set; }
    public string? GenreName { get; set; }
    public string? AuthorName { get; set; }
    public bool UseBookImage { get; set; }
    public List<TopicSeedDto> Topics { get; set; } = new();
}

public class TopicSeedDto
{
    public string Tittle { get; set; } = null!;
    public string Content { get; set; } = null!;
    public List<MessageSeedDto> Messages { get; set; } = new();
}

public class MessageSeedDto
{
    public string Content { get; set; } = null!;
}
