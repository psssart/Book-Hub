using Base.Domain;

namespace App.Domain.Entities;

public class Author : BaseEntityId
{
    public string Name { get; set; } = default!;
    public int Age { get; set; }
    public string Biography { get; set; } = default!;
    public byte[]? imageData { get; set; }
}