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

    public IActionResult Index(
        string? searchInput,
        string? sortBy,
        string? sortDirection,
        string? selectedAuthorsGuidsJson,
        string? selectedGenresGuidsJson,
        string? selectedPublishersGuidsJson,
        string? selectedWarehousesGuidsJson)
    {
        try
        {
            IQueryable<Book> books = _context.Books.Include(b => b.Publisher);
            IQueryable<Author> authors = _context.Authors;
            IQueryable<Rating> ratings = _context.Ratings;
            IQueryable<BookAuthor> bookAuthors = _context.BooksAuthors.Include(ba => ba.Author);
            IQueryable<BookGenre> bookGenres = _context.BooksGenres.Include(bg => bg.Genre);
            IQueryable<BookWarehouses> bookWarehouses = _context.BooksWarehouses.Include(bw => bw.Warehouse);

            if (!string.IsNullOrWhiteSpace(searchInput) || User.Identity?.IsAuthenticated == true)
            {
                // books by title
                books = books.Where(b => EF.Functions.ILike(b.Tittle, $"%{searchInput}%"));

                // narrow relations to found books
                var bookIdsQ = books.Select(b => b.Id);

                bookAuthors = bookAuthors.Where(ba => bookIdsQ.Contains(ba.BookId));
                bookGenres = bookGenres.Where(bg => bookIdsQ.Contains(bg.BookId));
                bookWarehouses = bookWarehouses.Where(bw => bookIdsQ.Contains(bw.BookId));
                ratings = ratings.Where(r => bookIdsQ.Contains(r.BookId));

                // filters
                if (!string.IsNullOrWhiteSpace(selectedPublishersGuidsJson) && selectedPublishersGuidsJson != "[]")
                {
                    var ids = JsonConvert.DeserializeObject<string[]>(selectedPublishersGuidsJson) ??
                              Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();
                    books = books.Where(b => guidIds.Contains(b.PublisherId));
                }

                if (!string.IsNullOrWhiteSpace(selectedWarehousesGuidsJson) && selectedWarehousesGuidsJson != "[]")
                {
                    var ids = JsonConvert.DeserializeObject<string[]>(selectedWarehousesGuidsJson) ??
                              Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();

                    var bookIds = bookWarehouses
                        .Where(bw => guidIds.Contains(bw.WarehouseId))
                        .Select(bw => bw.BookId);

                    books = books.Where(b => bookIds.Contains(b.Id));
                }

                if (!string.IsNullOrWhiteSpace(selectedAuthorsGuidsJson) && selectedAuthorsGuidsJson != "[]")
                {
                    var ids = JsonConvert.DeserializeObject<string[]>(selectedAuthorsGuidsJson) ??
                              Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();

                    var bookIds = bookAuthors
                        .Where(ba => guidIds.Contains(ba.AuthorId))
                        .Select(ba => ba.BookId);

                    books = books.Where(b => bookIds.Contains(b.Id));
                }

                if (!string.IsNullOrWhiteSpace(selectedGenresGuidsJson) && selectedGenresGuidsJson != "[]")
                {
                    var ids = JsonConvert.DeserializeObject<string[]>(selectedGenresGuidsJson) ?? Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();

                    var bookIds = bookGenres
                        .Where(bg => guidIds.Contains(bg.GenreId))
                        .Select(bg => bg.BookId);

                    books = books.Where(b => bookIds.Contains(b.Id));
                }

                // authors by name
                authors = authors.Where(a => EF.Functions.ILike(a.Name, $"%{searchInput}%"));
            }

            // sort
            List<Book> booksList;
            var ascending = string.Equals(sortDirection, "ascending", StringComparison.OrdinalIgnoreCase);

            switch ((sortBy ?? "none").ToLowerInvariant())
            {
                case "price":
                    booksList = ascending
                        ? books.OrderBy(b => b.Price).ToList()
                        : books.OrderByDescending(b => b.Price).ToList();
                    break;

                case "year":
                    booksList = ascending
                        ? books.OrderBy(b => b.ReleaseYear).ToList()
                        : books.OrderByDescending(b => b.ReleaseYear).ToList();
                    break;

                case "rating":
                {
                    var avgRatings = ratings
                        .GroupBy(r => r.BookId)
                        .Select(g => new { BookId = g.Key, Avg = g.Average(r => r.Value) });

                    var query = from b in books
                        join ar in avgRatings on b.Id equals ar.BookId into gj
                        from r in gj.DefaultIfEmpty()
                        select new { Book = b, Avg = (double?)r.Avg };

                    booksList = ascending
                        ? query.OrderBy(x => x.Avg == null).ThenBy(x => x.Avg).Select(x => x.Book).ToList()
                        : query.OrderByDescending(x => x.Avg ?? double.MinValue).Select(x => x.Book).ToList();
                    break;
                }

                default:
                    booksList = books.ToList();
                    break;
            }

            var vm = new HomeViewModel
            {
                Books = booksList,
                Authors = authors.ToList(),
                Ratings = ratings.ToList(),
                BookAuthors = bookAuthors.ToList(),
                BookGenres = bookGenres.ToList(),
                BookWarehouses = bookWarehouses.ToList(),
                SearchInput = searchInput,
                ShowResults = booksList.Any() || authors.Any()
            };

            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase);

            if (isAjax)
                return PartialView("_SearchResults", vm);

            if (User.Identity?.IsAuthenticated == true)
                return View("Index_auth", vm);

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Home/Index crashed");
            Response.StatusCode = 500;
            return Content($"Server error: {ex.Message}");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}