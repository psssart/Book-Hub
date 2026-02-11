using Base.Domain;
using NetTopologySuite.Geometries;

namespace App.Domain.Entities;

public class Warehouse : BaseEntityId
{
    public string Name { get; set; } = default!;
    public double GpsX { get; set; }
    public double GpsY { get; set; }

    public string Country { get; set; } = default!;

    public Point? Location { get; set; }
}