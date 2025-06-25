using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain.Entities;

public class Discussion : BaseEntityId, IDomainAppUser<AppUser>
{
    public Guid? BookId { get; set; }
    public Book? Book { get; set; }
    public Guid? GenreId { get; set; }
    public Genre? Genre { get; set; }
    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public string Tittle { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreationTime { get; set; }
    public byte[]? imageData { get; set; }
}