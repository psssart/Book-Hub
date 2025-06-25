using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain.Entities;

public class Rating : BaseEntityId, IDomainAppUser<AppUser>
{
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public float Value { get; set; }
    public string Comment { get; set; } = default!;
}