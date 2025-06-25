using App.Domain.Identity;
using Base.Contracts.Domain;

namespace App.DAL.DTO;

public class Message: IDomainEntityId, IDomainAppUser<AppUser>
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Topic? Topic { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreationTime { get; set; }
}