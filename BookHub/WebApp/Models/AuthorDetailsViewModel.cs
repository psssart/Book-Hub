using App.Domain.Entities;

namespace WebApp.Models;

public class AuthorDetailsViewModel
{
    public Author Author { get; set; } = default!;
    public HomeViewModel Search { get; set; } = new();
}