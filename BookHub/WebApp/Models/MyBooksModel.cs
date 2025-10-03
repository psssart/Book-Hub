using App.Domain.Address_Tables;
using App.Domain.Entities;

namespace WebApp.Models;

public class MyBooksModel
{
    public List<Book> Books { get; set; } = default!;
    public List<BookGenre> BookGenres { get; set; } = default!;
    public List<BookAuthor> BookAuthors { get; set; } = default!;
    public List<Rating> Ratings { get; set; } = default!;
    public Dictionary<Guid, List<Purchase>> PurchasesByBookId { get; set; } = new();
}