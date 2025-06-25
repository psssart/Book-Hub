using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Contracts.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Entities;

namespace WebApp.Controllers
{
    public class DiscussionsController : Controller
    {
        private readonly AppDbContext _context;

        public DiscussionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Discussions
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Discussions.Include(d => d.AppUser).Include(d => d.Author).Include(d => d.Book).Include(d => d.Genre);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Discussions/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions
                .Include(d => d.AppUser)
                .Include(d => d.Author)
                .Include(d => d.Book)
                .Include(d => d.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discussion == null)
            {
                return NotFound();
            }

            // Load the associated topics for the discussion
            /*var topics = await _uow.Topics.GetAllByDiscussionIdAsync(discussion.Id);*/
             var topics = _context.Topics
                 .Where(t => t.DiscussionId == discussion.Id);

            // Load messages related to the topics within the discussion
            /*var messages2 = await _uow.Messages.GetAllByTopicsAsync(topics);*/
             var messages = await _context.Messages
                 .Where(m => topics.Select(t => t.Id).Contains(m.TopicId))
                 .ToListAsync();

            // Pass the messages to the view
            ViewData["Messages"] = messages.ToList();

            // Pass the topics to the view
            ViewData["Topics"] = await topics.ToListAsync();
            return View(discussion);
        }

        // GET: Discussions/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name");
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle");
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Discussions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,GenreId,AuthorId,AppUserId,Tittle,Description,Id")] Discussion discussion, IFormFile imageData)
        {
            if (ModelState.IsValid)
            {
                
                if (imageData != null && imageData.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageData.CopyToAsync(memoryStream);
                        discussion.imageData = memoryStream.ToArray();
                    }
                }
                
                discussion.Id = Guid.NewGuid();
                discussion.CreationTime = DateTime.UtcNow;
                _context.Add(discussion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", discussion.AppUserId);
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", discussion.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", discussion.BookId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", discussion.GenreId);
            return View(discussion);
        }

        // GET: Discussions/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions.FindAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", discussion.AppUserId);
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", discussion.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", discussion.BookId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", discussion.GenreId);
            return View(discussion);
        }

        // POST: Discussions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("BookId,GenreId,AuthorId,AppUserId,Tittle,Description,Id")] Discussion discussion, IFormFile imageData)
        {
            if (id != discussion.Id)
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
                        discussion.imageData = memoryStream.ToArray();
                    }
                }
                
                try
                {
                    _context.Update(discussion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscussionExists(discussion.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", discussion.AppUserId);
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", discussion.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Tittle", discussion.BookId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", discussion.GenreId);
            return View(discussion);
        }

        // GET: Discussions/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions
                .Include(d => d.AppUser)
                .Include(d => d.Author)
                .Include(d => d.Book)
                .Include(d => d.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discussion == null)
            {
                return NotFound();
            }

            return View(discussion);
        }

        // POST: Discussions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var discussion = await _context.Discussions.FindAsync(id);
            if (discussion != null)
            {
                _context.Discussions.Remove(discussion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DiscussionExists(Guid id)
        {
            return _context.Discussions.Any(e => e.Id == id);
        }
    }
}
