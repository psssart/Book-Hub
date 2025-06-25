using App.Domain.Entities;
using Base.Domain;

namespace App.Domain.Address_Tables;

public class BookAuthor : BaseEntityId
{
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
}