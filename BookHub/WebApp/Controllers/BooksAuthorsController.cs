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
    public class BooksAuthorsController : Controller
    {
        private readonly AppDbContext _context;

        public BooksAuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BooksAuthors
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.BooksAuthors.Include(b => b.Author).Include(b => b.Book);
            return View(await appDbContext.ToListAsync());
        }

        // GET: BooksAuthors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookAuthor = await _context.BooksAuthors
                .Include(b => b.Author)
                .Include(b => b.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookAuthor == null)
            {
                return NotFound();
            }

            return View(bookAuthor);
        }

        // GET: BooksAuthors/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name");
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle");
            return View();
        }

        // POST: BooksAuthors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,AuthorId,Id")] BookAuthor bookAuthor)
        {
            if (ModelState.IsValid)
            {
                bookAuthor.Id = Guid.NewGuid();
                _context.Add(bookAuthor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", bookAuthor.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookAuthor.BookId);
            return View(bookAuthor);
        }

        // GET: BooksAuthors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookAuthor = await _context.BooksAuthors.FindAsync(id);
            if (bookAuthor == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", bookAuthor.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookAuthor.BookId);
            return View(bookAuthor);
        }

        // POST: BooksAuthors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("BookId,AuthorId,Id")] BookAuthor bookAuthor)
        {
            if (id != bookAuthor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookAuthor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookAuthorExists(bookAuthor.Id))
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
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", bookAuthor.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookAuthor.BookId);
            return View(bookAuthor);
        }

        // GET: BooksAuthors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookAuthor = await _context.BooksAuthors
                .Include(b => b.Author)
                .Include(b => b.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookAuthor == null)
            {
                return NotFound();
            }

            return View(bookAuthor);
        }

        // POST: BooksAuthors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var bookAuthor = await _context.BooksAuthors.FindAsync(id);
            if (bookAuthor != null)
            {
                _context.BooksAuthors.Remove(bookAuthor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookAuthorExists(Guid id)
        {
            return _context.BooksAuthors.Any(e => e.Id == id);
        }
    }
}
