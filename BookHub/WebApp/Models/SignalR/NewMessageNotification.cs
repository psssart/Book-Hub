namespace WebApp.Models.SignalR;

public class NewMessageNotification
{
    public Guid MessageId { get; set; }
    public Guid TopicId { get; set; }
    public string Content { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}
