using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain.Entities;

public class Message : BaseEntityId, IDomainAppUser<AppUser>
{
    public Guid TopicId { get; set; }
    public Topic? Topic { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}