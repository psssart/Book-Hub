namespace WebApp.Models.SignalR;

public class NewMessageNotification
{
    public Guid MessageId { get; set; }
    public Guid TopicId { get; set; }
    public Guid AppUserId { get; set; }
    public string Content { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? UserAvatarBase64 { get; set; }
    public DateTime CreationTime { get; set; }
}
