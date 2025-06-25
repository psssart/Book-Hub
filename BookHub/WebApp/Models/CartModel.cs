using App.Domain.Entities;

namespace WebApp.Models;

public class CartModel
{
    public List<Book> Books { get; set; } = default!;
    public bool IsBought { get; set; } = default!;
}