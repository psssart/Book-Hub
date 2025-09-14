using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Address_Tables;
using App.Domain.Entities;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Authors.ToListAsync());
        }

    // GET: Authors/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var author = await _context.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id.Value);

        if (author == null) return NotFound();
        
        var books = await _context.BooksAuthors
            .AsNoTracking()
            .Include(ba => ba.Book)
            .Select(ba => ba.Book)
            .Where(b => b != null)
            .Distinct()
            .ToListAsync();

        var bookIds = books.Select(b => b!.Id).ToList();
        
        var ratings = bookIds.Count == 0
            ? new List<Rating>()
            : await _context.Ratings
                .AsNoTracking()
                .Where(r => bookIds.Contains(r.BookId))
                .ToListAsync();

        var bookAuthors = bookIds.Count == 0
            ? new List<BookAuthor>()
            : await _context.BooksAuthors
                .AsNoTracking()
                .Where(ba => bookIds.Contains(ba.BookId))
                .Include(ba => ba.Author)
                .ToListAsync();

        var genres = bookIds.Count == 0
            ? new List<BookGenre>()
            : await _context.BooksGenres
                .AsNoTracking()
                .Where(bg => bookIds.Contains(bg.BookId))
                .Include(bg => bg.Genre)
                .ToListAsync();

        var vm = new AuthorDetailsViewModel
        {
            Author = author,
            Search = new HomeViewModel
            {
                Books = books!,
                Ratings = ratings,
                BookAuthors = bookAuthors,
                BookGenres = genres,
                BookWarehouses = [],
                Authors = new List<Author> { author },
                SearchInput = string.Empty,
                ShowResults = true,
                WithAuthors = false
            }
        };

        return View(vm);
    }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Age,Biography,Id")] Author author)
        {
            if (ModelState.IsValid)
            {
                author.Id = Guid.NewGuid();
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Age,Biography,Id")] Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(Guid id)
        {
            return _context.Authors.Any(e => e.Id == id);
        }
    }
}
