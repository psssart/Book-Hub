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
    public class BooksWarehousesController : Controller
    {
        private readonly AppDbContext _context;

        public BooksWarehousesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BooksWarehouses
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.BooksWarehouses.Include(b => b.Book).Include(b => b.Warehouse);
            return View(await appDbContext.ToListAsync());
        }

        // GET: BooksWarehouses/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookWarehouses = await _context.BooksWarehouses
                .Include(b => b.Book)
                .Include(b => b.Warehouse)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookWarehouses == null)
            {
                return NotFound();
            }

            return View(bookWarehouses);
        }

        // GET: BooksWarehouses/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle");
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name");
            return View();
        }

        // POST: BooksWarehouses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,WarehouseId,Id")] BookWarehouses bookWarehouses)
        {
            if (ModelState.IsValid)
            {
                bookWarehouses.Id = Guid.NewGuid();
                _context.Add(bookWarehouses);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookWarehouses.BookId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", bookWarehouses.WarehouseId);
            return View(bookWarehouses);
        }

        // GET: BooksWarehouses/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookWarehouses = await _context.BooksWarehouses.FindAsync(id);
            if (bookWarehouses == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookWarehouses.BookId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", bookWarehouses.WarehouseId);
            return View(bookWarehouses);
        }

        // POST: BooksWarehouses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("BookId,WarehouseId,Id")] BookWarehouses bookWarehouses)
        {
            if (id != bookWarehouses.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookWarehouses);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookWarehousesExists(bookWarehouses.Id))
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
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", bookWarehouses.BookId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", bookWarehouses.WarehouseId);
            return View(bookWarehouses);
        }

        // GET: BooksWarehouses/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookWarehouses = await _context.BooksWarehouses
                .Include(b => b.Book)
                .Include(b => b.Warehouse)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookWarehouses == null)
            {
                return NotFound();
            }

            return View(bookWarehouses);
        }

        // POST: BooksWarehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var bookWarehouses = await _context.BooksWarehouses.FindAsync(id);
            if (bookWarehouses != null)
            {
                _context.BooksWarehouses.Remove(bookWarehouses);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookWarehousesExists(Guid id)
        {
            return _context.BooksWarehouses.Any(e => e.Id == id);
        }
    }
}
