using App.Domain.Identity;

namespace App.DTO.v1_0;

public class Message
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Topic? Topic { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}