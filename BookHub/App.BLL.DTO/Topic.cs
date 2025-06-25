using App.Domain.Entities;
using App.Domain.Identity;
using Base.Contracts.Domain;

namespace App.BLL.DTO;

public class Topic: IDomainEntityId, IDomainAppUser<AppUser>
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