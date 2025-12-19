namespace WebApp.Models.SignalR;

public class NewTopicNotification
{
    public Guid TopicId { get; set; }
    public Guid DiscussionId { get; set; }
    public string Tittle { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}
