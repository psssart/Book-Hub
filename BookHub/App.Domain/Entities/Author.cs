using Base.Domain;
using NpgsqlTypes;

namespace App.Domain.Entities;

public class Author : BaseEntityId
{
    public string Name { get; set; } = default!;
    public int Age { get; set; }
    public string Biography { get; set; } = default!;
    public byte[]? imageData { get; set; }
    
    public NpgsqlTsVector SearchVector { get; private set; } = default!;
}