using Base.Domain;

namespace App.Domain.Entities;

public class Book : BaseEntityId
{
    public Guid PublisherId { get; set; }
    public Publisher? Publisher { get; set; }
    public int ReleaseYear { get; set; }
    
    public string Tittle { get; set; } = default!;
    public float Price { get; set; }
    public string Description { get; set; } = default!;
    public byte[]? imageData { get; set; }
}