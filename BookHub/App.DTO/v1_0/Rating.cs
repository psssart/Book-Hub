using App.Domain.Entities;
using App.Domain.Identity;

namespace App.DTO.v1_0;

public class Rating
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public float Value { get; set; }
    public string Comment { get; set; } = default!;
}