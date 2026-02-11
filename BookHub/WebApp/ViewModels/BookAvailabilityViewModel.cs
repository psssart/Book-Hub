using App.Domain.Address_Tables;
using App.Domain.Entities;

namespace WebApp.ViewModels;

public class BookAvailabilityViewModel
{
    public Book Book { get; set; } = default!;
    public List<BookWarehouses> BookWarehouses { get; set; } = new();
    public List<Warehouse> AllWarehouses { get; set; } = new();
    public List<string> Countries { get; set; } = new();
}
