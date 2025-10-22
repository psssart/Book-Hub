using System.Diagnostics;
using App.DAL.EF;
using App.Domain.Address_Tables;
using App.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Text.RegularExpressions;
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

            bool? userAuthenticated = User.Identity?.IsAuthenticated;
            bool emptySearchInput = string.IsNullOrWhiteSpace(searchInput);
            
            var tokens = Tokenize(searchInput);
            var containsPatterns   = tokens.Select(t => "%" + t + "%").ToArray();
            var prefixPatterns     = tokens.Select(t => t + "%").ToArray();
            var wordStartPatterns  = tokens.Select(t => "% " + t + "%").ToArray();
            var exactPhrasePattern = string.IsNullOrWhiteSpace(searchInput) ? null : searchInput;
            
            
            var relaxed3TokenPatterns = tokens
                .Where(t => t.Length >= 3)
                .Select(t => "%" + t.Substring(0, 3) + "%")
                .Distinct()
                .ToArray();

            var relaxed3WholeInputPattern = (!string.IsNullOrWhiteSpace(searchInput) && searchInput.Length >= 3)
                ? "%" + searchInput.Substring(0, 3) + "%"
                : null;
            if (!emptySearchInput || userAuthenticated == true)
            {
                // 1) strict: any token contained in title
                if (containsPatterns.Length > 0)
                {
                    books = books.Where(b => containsPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)));
                }
                else if (!string.IsNullOrWhiteSpace(searchInput))
                {
                    var broad = "%" + searchInput + "%";
                    books = books.Where(b => EF.Functions.ILike(b.Tittle, broad));
                }

                // 2) fallback if nothing found: prefix/word-start
                if (!books.Any() && tokens.Length > 0)
                {
                    books = _context.Books.Include(b => b.Publisher)
                        .Where(b =>
                            prefixPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)) ||
                            wordStartPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)));
                }
                
                // 3) fallback if still empty: relaxed 3-char roots
                if (!books.Any())
                {
                    if (relaxed3TokenPatterns.Length > 0)
                    {
                        books = _context.Books.Include(b => b.Publisher)
                            .Where(b => relaxed3TokenPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)));
                    }
                    else if (relaxed3WholeInputPattern != null)
                    {
                        var rp = relaxed3WholeInputPattern;
                        books = _context.Books.Include(b => b.Publisher)
                            .Where(b => EF.Functions.ILike(b.Tittle, rp));
                    }
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

                // Authors by name with tokens
                if (containsPatterns.Length > 0)
                {
                    authors = authors.Where(a => containsPatterns.Any(p => EF.Functions.ILike(a.Name, p)));
                }
                else if (!string.IsNullOrWhiteSpace(searchInput))
                {
                    var broad = "%" + searchInput + "%";
                    authors = authors.Where(a => EF.Functions.ILike(a.Name, broad));
                }

                if (!authors.Any() && tokens.Length > 0)
                {
                    authors = _context.Authors
                        .Where(a =>
                            prefixPatterns.Any(p => EF.Functions.ILike(a.Name, p)) ||
                            wordStartPatterns.Any(p => EF.Functions.ILike(a.Name, p)));
                }

                if (!authors.Any())
                {
                    if (relaxed3TokenPatterns.Length > 0)
                    {
                        authors = _context.Authors
                            .Where(a => relaxed3TokenPatterns.Any(p => EF.Functions.ILike(a.Name, p)));
                    }
                    else if (relaxed3WholeInputPattern != null)
                    {
                        var rp = relaxed3WholeInputPattern;
                        authors = _context.Authors
                            .Where(a => EF.Functions.ILike(a.Name, rp));
                    }
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
                    if (!emptySearchInput)
                    {
                        // Boolean tiers; EF translates bool OrderBy to CASE WHEN in SQL.
                        booksList = books
                            // 1) exact phrase match
                            .OrderByDescending(b => exactPhrasePattern != null && EF.Functions.ILike(b.Tittle, exactPhrasePattern))
                            // 2) starts-with any token
                            .ThenByDescending(b => prefixPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)))
                            // 3) word-start boost (naive)
                            .ThenByDescending(b => wordStartPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)))
                            // 4) contains any token
                            .ThenByDescending(b => containsPatterns.Any(p => EF.Functions.ILike(b.Tittle, p)))
                            // tie-breaker
                            .ThenBy(b => b.Tittle)
                            .ToList();
                    }
                    else
                    {
                        booksList = books.ToList();
                    }
                    break;
                }
            }

            // Authors: order by relevance if searching
            IQueryable<Author> authorsOrdered = authors;
            if (!emptySearchInput)
            {
                authorsOrdered = authors
                    .OrderByDescending(a => exactPhrasePattern != null && EF.Functions.ILike(a.Name, exactPhrasePattern))
                    .ThenByDescending(a => prefixPatterns.Any(p => EF.Functions.ILike(a.Name, p)))
                    .ThenByDescending(a => wordStartPatterns.Any(p => EF.Functions.ILike(a.Name, p)))
                    .ThenByDescending(a => containsPatterns.Any(p => EF.Functions.ILike(a.Name, p)))
                    .ThenBy(a => a.Name);
            }
            
            var vm = new HomeViewModel
            {
                Books = booksList,
                Authors = authorsOrdered.ToList(),
                Ratings = ratings.ToList(),
                BookAuthors = bookAuthors.ToList(),
                BookGenres = bookGenres.ToList(),
                BookWarehouses = bookWarehouses.ToList(),
                SearchInput = searchInput,
                ShowResults = (userAuthenticated == false && !emptySearchInput) || 
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
    
    // Splits user input into normalized tokens
    private static string[] Tokenize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return Array.Empty<string>();
        return Regex
            .Split(input.Trim(), @"[^\p{L}\p{N}]+", RegexOptions.CultureInvariant)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .ToArray();
    }
}