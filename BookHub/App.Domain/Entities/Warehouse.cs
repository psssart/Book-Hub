using Base.Domain;

namespace App.Domain.Entities;

public class Warehouse : BaseEntityId
{
    public string Name { get; set; } = default!;
    public double GpsX { get; set; }
    public double GpsY { get; set; }
}