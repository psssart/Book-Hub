using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Entities;
using System.Text.Json;
using App.Contracts.DAL;
using App.Domain.Address_Tables;
using App.Domain.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BooksController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Books.Include(b => b.Publisher);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            
            var bookAuthors = await _context.BooksAuthors
                .Where(ba => ba.BookId == book.Id)
                .Include(ba => ba.Author)
                .ToListAsync();
            var bookGenres = await _context.BooksGenres
                .Where(ba => ba.BookId == book.Id)
                .Include(ba => ba.Genre)
                .ToListAsync();
            var bookWarehouses = await _context.BooksWarehouses
                .Where(ba => ba.BookId == book.Id)
                .Include(ba => ba.Warehouse)
                .ToListAsync();
            var bookRatings = await _context.Ratings.Where(r => r.BookId.Equals(book.Id))
                .Include(r => r.AppUser)
                .ToListAsync();
            ViewData["BookAuthors"] = bookAuthors;
            ViewData["BookGenres"] = bookGenres;
            ViewData["BookWarehouses"] = bookWarehouses;
            ViewData["Ratings"] = bookRatings.ToList();
            
            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name");
            return View();
        }
        
        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PublisherId,Tittle,Price,ReleaseYear,Description,Id,imageData")] Book book, IFormFile imageData)
        {
            if (ModelState.IsValid)
            {
                
                if (imageData != null && imageData.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageData.CopyToAsync(memoryStream);
                        book.imageData = memoryStream.ToArray();
                    }
                }
                
                book.Id = Guid.NewGuid();
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublisherId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublisherId);
            return View(book);
        }
        
        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("PublisherId,Tittle,Price,Description,Id")] Book book, IFormFile imageData)
        {
            if (id != book.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                
                if (imageData != null && imageData.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageData.CopyToAsync(memoryStream);
                        book.imageData = memoryStream.ToArray();
                    }
                }
                
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublisherId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(Guid id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddToTempCart(Guid id)
        {
            // Check if user is authorized
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["AlertMessage"] = "You are unauthorized!";
                return RedirectToAction("Details", new { id = id }); // Redirect to the Details page with the specified book ID
            }
            
            // Check if there is an existing PurchasedBook with the same AppUserId and BookId
            var isBookBought = await _context.PurchasedBooks.Include(pb => pb.Purchase)
                .AnyAsync(pb => pb.Purchase!.AppUserId == currentUser.Id && pb.BookId == id);
            if (isBookBought)
            {
                TempData["CartMessage"] = "You already bought this book!";
                return RedirectToAction("Details", new { id = id });
            }
            
            // Get current cart from session
            List<Guid> tempCart = GetTempCart();

            // Check if book is already in the shopping cart
            if (tempCart.Contains(id))
            {
                TempData["CartMessage"] = "Book is already in your cart!";
                return RedirectToAction("Details", new { id = id });
            }
            
            // Adding a new book
            tempCart.Add(id);

            // Save the updated cart in the session
            SetTempCart(tempCart);

            // Set the message to TempData
            TempData["CartMessage"] = "Book added to Cart";
            
            return RedirectToAction("Details", new { id = id });
        }
        
        [HttpPost]
        public async Task<IActionResult> SubscribeOnBook(Guid id)
        {
            // Check if user is authorized
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["AlertMessage"] = "You are unauthorized!";
                return RedirectToAction("Details", new { id = id }); // Redirect to the Details page with the specified book ID
            }
            
            // Check if there is an existing UserSubscription with the same AppUserId and BookId
            bool subscriptionExists = _context.UsersSubscriptions
                .Any(us => us.AppUserId == currentUser.Id && us.BookId == id);
            
            if (subscriptionExists)
            {
                TempData["CartMessage"] = "You are already subscribed";
                return RedirectToAction("Details", new { id = id });
            }
            
            UserSubscription userSubscription = new UserSubscription()
            {
                Id = Guid.NewGuid(),
                AppUserId = currentUser.Id,
                BookId = id
            };
            
            _context.Add(userSubscription);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }
        
        private List<Guid> GetTempCart()
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
        
    }
}
