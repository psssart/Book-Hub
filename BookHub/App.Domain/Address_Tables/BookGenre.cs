using App.Domain.Entities;
using Base.Domain;

namespace App.Domain.Address_Tables;

public class BookGenre : BaseEntityId
{
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    public Guid GenreId { get; set; }
    public Genre? Genre { get; set; }
}