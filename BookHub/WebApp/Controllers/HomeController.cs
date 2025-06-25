using System.Diagnostics;
using App.DAL.EF;
using App.Domain.Address_Tables;
using App.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index(string searchInput, string sortBy, string sortDirection, string selectedAuthorsGuidsJson,
        string selectedGenresGuidsJson, string selectedPublishersGuidsJson, string selectedWarehousesGuidsJson)
    {
        IQueryable<Book> booksQuery = _context.Books.Include(b => b.Publisher);
        IQueryable<Author> authorsQuery = _context.Authors;
        IQueryable<Rating> ratingsQuery = _context.Ratings;
        IQueryable<BookAuthor> bookAuthorsQuery = _context.BooksAuthors;
        IQueryable<BookGenre> bookGenresQuery = _context.BooksGenres.Include(bg => bg.Genre);
        IQueryable<BookWarehouses> bookWarehousesQuery = _context.BooksWarehouses;

        if (!string.IsNullOrEmpty(searchInput))
        {
            // Filtering books by title
            booksQuery = booksQuery.Where(b => b.Tittle.ToLower().Contains(searchInput.ToLower()));
            var bookIds = booksQuery.Select(b => b.Id);
            
            bookAuthorsQuery = bookAuthorsQuery.Where(ba => bookIds.Contains(ba.BookId)).Include(ba => ba.Author);
            bookGenresQuery = bookGenresQuery.Where(bg => bookIds.Contains(bg.BookId)).Include(bg =>bg.Genre);
            bookWarehousesQuery = bookWarehousesQuery.Where(bw => bookIds.Contains(bw.BookId)).Include(bw => bw.Warehouse);
            
            //Filter books by Publishers
            if (!string.IsNullOrEmpty(selectedPublishersGuidsJson) && !selectedPublishersGuidsJson.Equals("[]"))
            {
                var selectedPublishersGuids = JsonConvert.DeserializeObject<string[]>(selectedPublishersGuidsJson);
                var publishersIds = Array.ConvertAll(selectedPublishersGuids, Guid.Parse);
                
                booksQuery = booksQuery.Where(b => publishersIds.Contains(b.PublisherId));
            }
            
            //Filter books by Warehouses
            if (!string.IsNullOrEmpty(selectedWarehousesGuidsJson) && !selectedWarehousesGuidsJson.Equals("[]"))
            {
                var selectedWarehousesGuids = JsonConvert.DeserializeObject<string[]>(selectedWarehousesGuidsJson);
                var warehousesIds = Array.ConvertAll(selectedWarehousesGuids, Guid.Parse);
                
                // Filter book warehouses by selected warehouses
                var bookSpecialWarehousesIds = bookWarehousesQuery
                    .Where(bw => warehousesIds.Contains(bw.WarehouseId))
                    .Select(bw => bw.BookId)
                    .ToList();
                
                booksQuery = booksQuery.Where(b => bookSpecialWarehousesIds.Contains(b.Id));
            }
            
            //Filter books by Authors
            if (!string.IsNullOrEmpty(selectedAuthorsGuidsJson) && !selectedAuthorsGuidsJson.Equals("[]"))
            {
                // Deserialize the JSON string to get the array of GUIDs
                var selectedAuthorsGuids = JsonConvert.DeserializeObject<string[]>(selectedAuthorsGuidsJson);
                var authorsIds = Array.ConvertAll(selectedAuthorsGuids, Guid.Parse);
                
                var bookSpecialAuthorsIds = bookAuthorsQuery
                    .Where(ba => authorsIds.Contains(ba.AuthorId))
                    .Select(ba => ba.BookId)
                    .ToList();
                
                // Filter books by selected authors
                booksQuery = booksQuery.Where(b => bookSpecialAuthorsIds.Contains(b.Id));
            }
            
            //Filter books by Genres
            if (!string.IsNullOrEmpty(selectedGenresGuidsJson) && !selectedGenresGuidsJson.Equals("[]"))
            {
                var selectedGenresGuids = JsonConvert.DeserializeObject<string[]>(selectedGenresGuidsJson);
                
                var genresIds = Array.ConvertAll(selectedGenresGuids, Guid.Parse);
                
                var bookSpecialGenresIds = bookGenresQuery
                    .Where(bg => genresIds.Contains(bg.GenreId))
                    .Select(bg => bg.BookId)
                    .ToList();
                
                booksQuery = booksQuery.Where(b => bookSpecialGenresIds.Contains(b.Id));
            }
            
            
            // Оставляем в ratingsQuery только те записи, где BookId присутствует в bookIds
            ratingsQuery = ratingsQuery.Where(r => bookIds.Contains(r.BookId));

            // Filtering authors by name
            authorsQuery = authorsQuery.Where(a => a.Name.ToLower().Contains(searchInput.ToLower()));
            List<Book> booksList = new List<Book>();
            
            
            // Apply sorting if sortBy and sortDirection are provided
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortDirection) && !sortBy.Equals("none"))
            {
                bool ascending = string.Equals(sortDirection, "ascending", StringComparison.OrdinalIgnoreCase);
                switch (sortBy.ToLower())
                {
                    case "price":
                        booksList = ascending ? booksQuery.OrderBy(b => b.Price).ToList() : booksQuery.OrderByDescending(b => b.Price).ToList();
                        break;
                    case "rating":
                        // Join booksQuery with ratingsQuery on BookId
                        var joinedQuery = booksQuery
                            .GroupJoin(ratingsQuery,
                                b => b.Id,
                                r => r.BookId,
                                (b, r) => new { Book = b, Rating = r.FirstOrDefault() });

                        // Separate books with and without ratings
                        var ratedBooksQuery = joinedQuery.Where(j => j.Rating != null);
                        var unratedBooksQuery = joinedQuery.Where(j => j.Rating == null);
                        
                        ratedBooksQuery = ascending ? ratedBooksQuery.OrderBy(j => j.Rating!.Value) : ratedBooksQuery.OrderByDescending(j => j.Rating!.Value);
                        
                        // Retrieve rated books and unrated books separately
                        var ratedBooks = ratedBooksQuery.Select(j => j.Book).ToList();
                        var unratedBooks = unratedBooksQuery.Select(j => j.Book).ToList();
                        
                        booksList = ascending ? unratedBooks.Concat(ratedBooks).ToList() : ratedBooks.Concat(unratedBooks).ToList();
                        break;
                    case "year":
                        booksList = ascending ? booksQuery.OrderBy(b => b.ReleaseYear).ToList() : booksQuery.OrderByDescending(b => b.ReleaseYear).ToList();
                        break;
                }
            }
            else
            {
                booksList = booksQuery.ToList();
            }
            
            var viewModel1 = new HomeViewModel()
            {
                Books = booksList,
                Authors = authorsQuery.ToList(),
                Ratings = ratingsQuery.ToList(),
                BookAuthors = bookAuthorsQuery.ToList(),
                BookGenres = bookGenresQuery.ToList(),
                BookWarehouses = bookWarehousesQuery.ToList(),
                SearchInput = searchInput, // Add a search variable to the view model
                ShowResults = !string.IsNullOrEmpty(searchInput) && (booksQuery.Any() || authorsQuery.Any()) // Determine if we need to show results
            };
            if (User.Identity!.IsAuthenticated)
            {
                return View("Index_auth", viewModel1);
            }
            return View(viewModel1);
        }
        
        var viewModel = new HomeViewModel()
        {
            Books = booksQuery.ToList(),
            Authors = authorsQuery.ToList(),
            Ratings = ratingsQuery.ToList(),
            BookAuthors = bookAuthorsQuery.ToList(),
            BookGenres = bookGenresQuery.ToList(),
            BookWarehouses = bookWarehousesQuery.ToList(),
            SearchInput = searchInput, // Add a search variable to the view model
            ShowResults = !string.IsNullOrEmpty(searchInput) && (booksQuery.Any() || authorsQuery.Any()) // Determine if we need to show results
        };

        
        if (User.Identity!.IsAuthenticated)
        {
            return View("Index_auth", viewModel);
        }
        return View(viewModel);
    }

    public IActionResult Index_auth(HomeViewModel viewModel)
    {
        return View(viewModel);
    }
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}