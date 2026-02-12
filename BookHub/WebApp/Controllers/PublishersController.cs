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
using Microsoft.AspNetCore.Authorization;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class PublishersController : Controller
    {
        private readonly AppDbContext _context;

        public PublishersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Publishers
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Publishers.ToListAsync());
        }

        // GET: Publishers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var publisher = await _context.Publishers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null) return NotFound();

            var books = await _context.Books
                .AsNoTracking()
                .Where(b => b.PublisherId == publisher.Id)
                .Include(b => b.Publisher)
                .ToListAsync();

            var bookIds = books.Select(b => b.Id).ToList();

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

            var vm = new PublisherDetailsViewModel
            {
                Publisher = publisher,
                BookCount = books.Count,
                Search = new HomeViewModel
                {
                    Books = books,
                    Ratings = ratings,
                    BookAuthors = bookAuthors,
                    BookGenres = genres,
                    BookWarehouses = [],
                    Authors = [],
                    SearchInput = string.Empty,
                    ShowResults = true,
                    WithAuthors = false
                }
            };

            return View(vm);
        }

        // GET: Publishers/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Publishers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,Id")] Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                publisher.Id = Guid.NewGuid();
                _context.Add(publisher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        // GET: Publishers/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }

        // POST: Publishers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Description,Id")] Publisher publisher)
        {
            if (id != publisher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publisher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublisherExists(publisher.Id))
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
            return View(publisher);
        }

        // GET: Publishers/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Publishers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher != null)
            {
                _context.Publishers.Remove(publisher);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublisherExists(Guid id)
        {
            return _context.Publishers.Any(e => e.Id == id);
        }
    }
}
