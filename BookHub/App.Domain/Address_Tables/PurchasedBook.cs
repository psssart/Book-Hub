using App.Domain.Entities;
using Base.Domain;

namespace App.Domain.Address_Tables;

public class PurchasedBook : BaseEntityId
{
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public Guid PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public bool BookHasRead { get; set; }
}