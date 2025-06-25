using System.Collections.Generic;
using App.Domain.Address_Tables;
using App.Domain.Entities;

namespace WebApp.Models
{
    public class HomeViewModel
    {
        public List<Book> Books { get; set; } = default!;
        public List<BookGenre> BookGenres { get; set; } = default!;
        public List<BookAuthor> BookAuthors { get; set; } = default!;
        public List<BookWarehouses> BookWarehouses { get; set; } = default!;
        public List<Author> Authors { get; set; } = default!;
        public List<Rating> Ratings { get; set; } = default!;
        public string SearchInput { get; set; } = default!;
        public bool ShowResults { get; set; } = default!;
    }
}