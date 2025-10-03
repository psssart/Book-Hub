using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Address_Tables;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using App.Contracts.DAL;
using App.Domain.Entities;
using App.Web.Infrastructure;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class PurchasedBooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PurchasedBooksController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> CreatePur()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized();
            }

            List<Guid> tempCart = GetTempCart();

            if (!tempCart.Any())
            {
                TempData.Error("Empty cart");
                return RedirectToAction("Index");
            }

            var booksInTempCart = await _context.Books.Where(b => tempCart.Contains(b.Id)).ToListAsync();

            // Calculate the total cost and other properties of the purchase
            double totalValue = booksInTempCart.Sum(b => b.Price);
            double totalDiscount = 0; // Calculate discount

            // Create a new purchase
            var newPurchase = new Purchase
            {
                AppUserId = currentUser.Id,
                Value = totalValue,
                Discount = totalDiscount,
            };

            _context.Purchases.Add(newPurchase);
            await _context.SaveChangesAsync();

            // Create a PurchasedBook for each book in the purchase
            foreach (var book in booksInTempCart)
            {
                var purchasedBook = new PurchasedBook
                {
                    BookId = book.Id,
                    PurchaseId = newPurchase.Id,
                };

                _context.PurchasedBooks.Add(purchasedBook);
            }

            await _context.SaveChangesAsync();

            // Clear session after completing purchase
            HttpContext.Session.Remove("TempCart");

            TempData.Success("Books purchased successfully");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult RemoveFromTempCart(Guid id)
        {
            // Get current cart from session
            List<Guid> tempCart = GetTempCart();

            // Remove a book from the cart
            tempCart.Remove(id);

            // Save the updated cart in the session
            SetTempCart(tempCart);

            return RedirectToAction("Create"); // Redirect to the same place we were
        }
        
        public List<Guid> GetTempCart()
        {
            byte[]? cartData = HttpContext.Session.Get("TempCart");
            return (cartData != null
                ? JsonSerializer.Deserialize<List<Guid>>(cartData)
                : new List<Guid>())!;
        }
        
        private void SetTempCart(List<Guid> tempCart)
        {
            byte[] cartData = JsonSerializer.SerializeToUtf8Bytes(tempCart);
            HttpContext.Session.Set("TempCart", cartData);
        }
        
        // GET: PurchasedBooks
        public async Task<IActionResult> Index()
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Get all the user's purchased books
            var purchasedItems = await _context.PurchasedBooks
                .AsNoTracking()
                .Include(pb => pb.Book)
                .Include(pb => pb.Purchase)
                .Where(pb => pb.Purchase!.AppUserId == currentUser.Id)
                .ToListAsync();
            
            var books = purchasedItems
                .Select(pb => pb.Book!)
                .DistinctBy(b => b.Id)
                .ToList();

            var bookIds = books.Select(b => b.Id).ToList();
            
            var purchasesByBookId = purchasedItems
                .GroupBy(pb => pb.BookId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Purchase!).ToList()
                );
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
            
            var viewModel = new MyBooksModel()
            {
                Books = books,
                Ratings = ratings,
                BookAuthors = bookAuthors,
                BookGenres = genres,
                PurchasesByBookId = purchasesByBookId
            };
            
            return View(viewModel);
        }

        // GET: PurchasedBooks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchasedBook = await _context.PurchasedBooks
                .Include(p => p.Book)
                .Include(p => p.Purchase)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchasedBook == null)
            {
                return NotFound();
            }

            return View(purchasedBook);
        }

        // GET: PurchasedBooks/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }
            
            List<Guid> tempCart = GetTempCart();

            var books = await _context.Books.Where(b => tempCart.Contains(b.Id)).ToListAsync();
            
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
            
            var viewModel = new CartModel()
            {
                Books = books,
                Ratings = ratings,
                BookAuthors = bookAuthors,
                BookGenres = genres,
                IsBought = false
            };
            
            // ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description");
            // ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id");
            return View(viewModel);
        }

        // POST: PurchasedBooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("BookId,PurchaseId,BookHasRead,Id")] PurchasedBook purchasedBook)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         purchasedBook.Id = Guid.NewGuid();
        //         _context.Add(purchasedBook);
        //         await _context.SaveChangesAsync();
        //         return RedirectToAction(nameof(Index));
        //     }
        //     ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description", purchasedBook.BookId);
        //     ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id", purchasedBook.PurchaseId);
        //     return View(purchasedBook);
        // }

        // GET: PurchasedBooks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchasedBook = await _context.PurchasedBooks.FindAsync(id);
            if (purchasedBook == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description", purchasedBook.BookId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id", purchasedBook.PurchaseId);
            return View(purchasedBook);
        }

        // POST: PurchasedBooks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("BookId,PurchaseId,BookHasRead,Id")] PurchasedBook purchasedBook)
        {
            if (id != purchasedBook.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchasedBook);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchasedBookExists(purchasedBook.Id))
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
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description", purchasedBook.BookId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id", purchasedBook.PurchaseId);
            return View(purchasedBook);
        }

        // GET: PurchasedBooks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchasedBook = await _context.PurchasedBooks
                .Include(p => p.Book)
                .Include(p => p.Purchase)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchasedBook == null)
            {
                return NotFound();
            }

            return View(purchasedBook);
        }

        // POST: PurchasedBooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var purchasedBook = await _context.PurchasedBooks.FindAsync(id);
            if (purchasedBook != null)
            {
                _context.PurchasedBooks.Remove(purchasedBook);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchasedBookExists(Guid id)
        {
            return _context.PurchasedBooks.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ToggleBookHasRead(Guid id)
        {
            var purchasedBook = await _context.PurchasedBooks.FindAsync(id);
            if (purchasedBook != null)
            {
                purchasedBook.BookHasRead = !purchasedBook.BookHasRead;
                _context.Update(purchasedBook);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
