using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain.Entities;

public class Topic : BaseEntityId, IDomainAppUser<AppUser>
{
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public Guid DiscussionId { get; set; }
    public Discussion? Discussion { get; set; }
    public string Tittle { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}