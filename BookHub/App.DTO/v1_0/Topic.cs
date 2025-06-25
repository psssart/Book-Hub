using App.Domain.Entities;
using App.Domain.Identity;

namespace App.DTO.v1_0;

public class Topic
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public Guid DiscussionId { get; set; }
    public Discussion? Discussion { get; set; }
    public string Tittle { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}