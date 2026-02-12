using App.Domain.Entities;

namespace WebApp.Models;

public class PublisherDetailsViewModel
{
    public Publisher Publisher { get; set; } = default!;
    public int BookCount { get; set; }
    public HomeViewModel Search { get; set; } = new();
}
