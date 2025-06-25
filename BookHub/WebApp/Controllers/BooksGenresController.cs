using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Address_Tables;

namespace WebApp.Controllers
{
    public class BooksGenresController : Controller
    {
        private readonly AppDbContext _context;

        public BooksGenresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BooksGenres
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.BooksGenres.Include(b => b.Book).Include(b => b.Genre);
            return View(await appDbContext.ToListAsync());
        }

        // GET: BooksGenres/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookGenre = await _context.BooksGenres
                .Include(b => b.Book)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookGenre == null)
            {
                return NotFound();
            }

            return View(bookGenre);
        }

        // GET: BooksGenres/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle");
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: BooksGenres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,GenreId,Id")] BookGenre bookGenre)
        {
            if (ModelState.IsValid)
            {
                bookGenre.Id = Guid.NewGuid();
                _context.Add(bookGenre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookGenre.BookId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", bookGenre.GenreId);
            return View(bookGenre);
        }

        // GET: BooksGenres/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookGenre = await _context.BooksGenres.FindAsync(id);
            if (bookGenre == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookGenre.BookId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", bookGenre.GenreId);
            return View(bookGenre);
        }

        // POST: BooksGenres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("BookId,GenreId,Id")] BookGenre bookGenre)
        {
            if (id != bookGenre.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookGenre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookGenreExists(bookGenre.Id))
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
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookGenre.BookId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", bookGenre.GenreId);
            return View(bookGenre);
        }

        // GET: BooksGenres/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookGenre = await _context.BooksGenres
                .Include(b => b.Book)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookGenre == null)
            {
                return NotFound();
            }

            return View(bookGenre);
        }

        // POST: BooksGenres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var bookGenre = await _context.BooksGenres.FindAsync(id);
            if (bookGenre != null)
            {
                _context.BooksGenres.Remove(bookGenre);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookGenreExists(Guid id)
        {
            return _context.BooksGenres.Any(e => e.Id == id);
        }
    }
}
