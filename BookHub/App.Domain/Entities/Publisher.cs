using Base.Domain;

namespace App.Domain.Entities;

public class Publisher : BaseEntityId
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}