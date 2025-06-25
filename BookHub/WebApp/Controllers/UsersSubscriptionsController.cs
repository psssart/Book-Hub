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
    public class UsersSubscriptionsController : Controller
    {
        private readonly AppDbContext _context;

        public UsersSubscriptionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UsersSubscriptions
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.UsersSubscriptions.Include(u => u.AppUser).Include(u => u.Book);
            return View(await appDbContext.ToListAsync());
        }

        // GET: UsersSubscriptions/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSubscription = await _context.UsersSubscriptions
                .Include(u => u.AppUser)
                .Include(u => u.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSubscription == null)
            {
                return NotFound();
            }

            return View(userSubscription);
        }

        // GET: UsersSubscriptions/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description");
            return View();
        }

        // POST: UsersSubscriptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppUserId,BookId,Id")] UserSubscription userSubscription)
        {
            if (ModelState.IsValid)
            {
                userSubscription.Id = Guid.NewGuid();
                userSubscription.CreationTime = DateTime.UtcNow;
                _context.Add(userSubscription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", userSubscription.AppUserId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description", userSubscription.BookId);
            return View(userSubscription);
        }

        // GET: UsersSubscriptions/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSubscription = await _context.UsersSubscriptions.FindAsync(id);
            if (userSubscription == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", userSubscription.AppUserId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description", userSubscription.BookId);
            return View(userSubscription);
        }

        // POST: UsersSubscriptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AppUserId,BookId,Id")] UserSubscription userSubscription)
        {
            if (id != userSubscription.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userSubscription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserSubscriptionExists(userSubscription.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", userSubscription.AppUserId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Description", userSubscription.BookId);
            return View(userSubscription);
        }

        // GET: UsersSubscriptions/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSubscription = await _context.UsersSubscriptions
                .Include(u => u.AppUser)
                .Include(u => u.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSubscription == null)
            {
                return NotFound();
            }

            return View(userSubscription);
        }

        // POST: UsersSubscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userSubscription = await _context.UsersSubscriptions.FindAsync(id);
            if (userSubscription != null)
            {
                _context.UsersSubscriptions.Remove(userSubscription);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserSubscriptionExists(Guid id)
        {
            return _context.UsersSubscriptions.Any(e => e.Id == id);
        }
    }
}
