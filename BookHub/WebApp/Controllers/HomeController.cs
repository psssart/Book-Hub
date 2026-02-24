using System.Diagnostics;
using App.DAL.EF;
using App.Domain.Address_Tables;
using App.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApp.Models;
using System.Text.RegularExpressions;

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

            bool? userAuthenticated = User.Identity?.IsAuthenticated;
            bool searchInputWasProvided = Request.Query.ContainsKey(nameof(searchInput));
            bool emptySearchInput = string.IsNullOrWhiteSpace(searchInput);
            
            if (!emptySearchInput || userAuthenticated == true)
            {
                if (!emptySearchInput)
                {
                    var q = searchInput!.Trim();
                    
                    var tokens = Regex
                        .Split(q, @"[^\p{L}\p{N}]+", RegexOptions.CultureInvariant)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim())
                        .Distinct()
                        .ToArray();

                    var loose = tokens.Length > 0 ? tokens[0] : q;
                    
                    var prefix3 = loose.Length >= 3 ? loose.Substring(0, 3) : loose;
                    
                    books = books
                        .Where(b =>
                            // 1) the full text matched
                            b.SearchVector.Matches(
                                EF.Functions.WebSearchToTsQuery(
                                    "simple",
                                    EF.Functions.Unaccent(q)
                                )
                            )
                            ||

                            // 2) string matches by keyword
                            EF.Functions.ILike(b.Tittle, $"%{loose}%") ||
                            EF.Functions.ILike(b.Description, $"%{loose}%") ||

                            // 3) match by shortened prefix (3 characters)
                            EF.Functions.ILike(b.Tittle, $"%{prefix3}%") ||
                            EF.Functions.ILike(b.Description, $"%{prefix3}%") ||

                            // 4) match on the entire user source string
                            EF.Functions.ILike(b.Tittle, $"%{q}%") ||
                            EF.Functions.ILike(b.Description, $"%{q}%") ||

                            // 5) similarity by trigrams with a threshold
                            EF.Functions.TrigramsSimilarity(b.Tittle, q) > 0.1
                        )
                        .OrderByDescending(b =>
                            b.SearchVector.Matches(
                                EF.Functions.WebSearchToTsQuery(
                                    "simple",
                                    EF.Functions.Unaccent(q)
                                )
                            )
                        )
                        .ThenByDescending(b =>
                            b.SearchVector.Rank(
                                EF.Functions.WebSearchToTsQuery(
                                    "simple",
                                    EF.Functions.Unaccent(q)
                                )
                            )
                        )
                        .ThenByDescending(b =>
                            EF.Functions.TrigramsSimilarity(b.Tittle, q)
                        );
                    
                    authors = authors
                        .Where(a =>
                            a.SearchVector.Matches(
                                EF.Functions.WebSearchToTsQuery(
                                    "simple",
                                    EF.Functions.Unaccent(q)
                                )
                            )
                            ||
                            
                            EF.Functions.ILike(a.Name, $"%{loose}%") ||
                            EF.Functions.ILike(a.Biography, $"%{loose}%") ||
                            
                            EF.Functions.ILike(a.Name, $"%{prefix3}%") ||
                            EF.Functions.ILike(a.Biography, $"%{prefix3}%") ||
                            
                            EF.Functions.ILike(a.Name, $"%{q}%") ||
                            EF.Functions.ILike(a.Biography, $"%{q}%") ||
                            
                            EF.Functions.TrigramsSimilarity(a.Name, q) > 0.1
                        )
                        .OrderByDescending(a =>
                            a.SearchVector.Matches(
                                EF.Functions.WebSearchToTsQuery(
                                    "simple",
                                    EF.Functions.Unaccent(q)
                                )
                            )
                        )
                        .ThenByDescending(a =>
                            a.SearchVector.Rank(
                                EF.Functions.WebSearchToTsQuery(
                                    "simple",
                                    EF.Functions.Unaccent(q)
                                )
                            )
                        )
                        .ThenByDescending(a =>
                            EF.Functions.TrigramsSimilarity(a.Name, q)
                        );
                }

                // narrow relations to found books
                var bookIdsQ = books.Select(b => b.Id);

                bookAuthors = bookAuthors.Where(ba => bookIdsQ.Contains(ba.BookId));
                bookGenres = bookGenres.Where(bg => bookIdsQ.Contains(bg.BookId));
                bookWarehouses = bookWarehouses.Where(bw => bookIdsQ.Contains(bw.BookId));
                ratings = ratings.Where(r => bookIdsQ.Contains(r.BookId));

                // filters
                if (!string.IsNullOrWhiteSpace(selectedPublishersGuidsJson) && selectedPublishersGuidsJson != "[]")
                {
                    var ids = JsonSerializer.Deserialize<string[]>(selectedPublishersGuidsJson) ??
                              Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();
                    books = books.Where(b => guidIds.Contains(b.PublisherId));
                }

                if (!string.IsNullOrWhiteSpace(selectedWarehousesGuidsJson) && selectedWarehousesGuidsJson != "[]")
                {
                    var ids = JsonSerializer.Deserialize<string[]>(selectedWarehousesGuidsJson) ??
                              Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();

                    var bookIds = bookWarehouses
                        .Where(bw => guidIds.Contains(bw.WarehouseId))
                        .Select(bw => bw.BookId);

                    books = books.Where(b => bookIds.Contains(b.Id));
                }

                if (!string.IsNullOrWhiteSpace(selectedAuthorsGuidsJson) && selectedAuthorsGuidsJson != "[]")
                {
                    var ids = JsonSerializer.Deserialize<string[]>(selectedAuthorsGuidsJson) ??
                              Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();

                    var bookIds = bookAuthors
                        .Where(ba => guidIds.Contains(ba.AuthorId))
                        .Select(ba => ba.BookId);

                    books = books.Where(b => bookIds.Contains(b.Id));
                }

                if (!string.IsNullOrWhiteSpace(selectedGenresGuidsJson) && selectedGenresGuidsJson != "[]")
                {
                    var ids = JsonSerializer.Deserialize<string[]>(selectedGenresGuidsJson) ?? Array.Empty<string>();
                    var guidIds = ids.Select(Guid.Parse).ToArray();

                    var bookIds = bookGenres
                        .Where(bg => guidIds.Contains(bg.GenreId))
                        .Select(bg => bg.BookId);

                    books = books.Where(b => bookIds.Contains(b.Id));
                }
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
                {
                    booksList = books.ToList();
                    break;
                }
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
                ShowResults = (userAuthenticated == false && searchInputWasProvided) || 
                              (userAuthenticated == true && (booksList.Any() || authors.Any()))
            };

            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase);

            if (isAjax)
                return PartialView("_SearchResults", vm);

            if (userAuthenticated == true)
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