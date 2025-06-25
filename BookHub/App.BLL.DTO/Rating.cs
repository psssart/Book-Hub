using App.Domain.Entities;
using App.Domain.Identity;
using Base.Contracts.Domain;

namespace App.BLL.DTO;

public class Rating: IDomainEntityId, IDomainAppUser<AppUser>
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public float Value { get; set; }
    public string Comment { get; set; } = default!;

}