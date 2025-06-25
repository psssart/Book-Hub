using App.Domain.Entities;
using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain.Address_Tables;

public class UserSubscription : BaseEntityId, IDomainAppUser<AppUser>
{
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public DateTime CreationTime { get; set; }
}