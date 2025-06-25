using App.Domain.Entities;
using Base.Domain;

namespace App.Domain.Address_Tables;

public class BookWarehouses : BaseEntityId
{
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
}