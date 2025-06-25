using Base.Domain;

namespace App.Domain.Entities;

public class Genre : BaseEntityId
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsMainGenre { get; set; }
}