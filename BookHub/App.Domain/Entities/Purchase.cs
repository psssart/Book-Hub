using Base.Contracts.Domain;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain.Entities;

public class Purchase : BaseEntityId, IDomainAppUser<AppUser>
{
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public double Value { get; set; }
    public double Discount { get; set; }
    public DateTime CreationTime { get; set; }
}