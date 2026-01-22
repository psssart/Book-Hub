using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using App.Contracts.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Entities;
using WebApp.Models;

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
        public async Task<IActionResult> Index(
            string? searchInput,
            string? sortBy,
            string? sortDirection,
            string? selectedAuthorsGuidsJson,
            string? selectedGenresGuidsJson,
            string? selectedBooksGuidsJson)
        {
            // Step 1: Base query with includes
            var discussionsQuery = _context.Discussions
                .Include(d => d.AppUser)
                .Include(d => d.Author)
                .Include(d => d.Book)
                .Include(d => d.Genre)
                .AsQueryable();

            // Step 2: Apply filters

            // Text search on discussion title/description
            if (!string.IsNullOrWhiteSpace(searchInput))
            {
                discussionsQuery = discussionsQuery.Where(d =>
                    EF.Functions.ILike(d.Tittle, $"%{searchInput}%") ||
                    EF.Functions.ILike(d.Description, $"%{searchInput}%"));
            }

            // Filter by Books
            if (!string.IsNullOrWhiteSpace(selectedBooksGuidsJson) && selectedBooksGuidsJson != "[]")
            {
                var bookIds = JsonSerializer.Deserialize<string[]>(selectedBooksGuidsJson)!
                    .Select(Guid.Parse).ToArray();
                discussionsQuery = discussionsQuery.Where(d =>
                    d.BookId.HasValue && bookIds.Contains(d.BookId.Value));
            }

            // Filter by Authors
            if (!string.IsNullOrWhiteSpace(selectedAuthorsGuidsJson) && selectedAuthorsGuidsJson != "[]")
            {
                var authorIds = JsonSerializer.Deserialize<string[]>(selectedAuthorsGuidsJson)!
                    .Select(Guid.Parse).ToArray();
                discussionsQuery = discussionsQuery.Where(d =>
                    d.AuthorId.HasValue && authorIds.Contains(d.AuthorId.Value));
            }

            // Filter by Genres
            if (!string.IsNullOrWhiteSpace(selectedGenresGuidsJson) && selectedGenresGuidsJson != "[]")
            {
                var genreIds = JsonSerializer.Deserialize<string[]>(selectedGenresGuidsJson)!
                    .Select(Guid.Parse).ToArray();
                discussionsQuery = discussionsQuery.Where(d =>
                    d.GenreId.HasValue && genreIds.Contains(d.GenreId.Value));
            }

            // Step 3: Execute discussion query first
            var discussions = await discussionsQuery.ToListAsync();
            var discussionIds = discussions.Select(d => d.Id).ToList();

            // Step 4: Compute metrics efficiently (avoid N+1 queries)

            // Single query for all topics grouped by discussion
            var topicsByDiscussion = await _context.Topics
                .Where(t => discussionIds.Contains(t.DiscussionId))
                .GroupBy(t => t.DiscussionId)
                .Select(g => new
                {
                    DiscussionId = g.Key,
                    TopicCount = g.Count(),
                    TopicUserIds = g.Select(t => t.AppUserId).Distinct().ToList(),
                    LastTopicTime = g.Max(t => (DateTime?)t.CreationTime)
                })
                .ToListAsync();

            // Single query for all topic IDs
            var topicIds = await _context.Topics
                .Where(t => discussionIds.Contains(t.DiscussionId))
                .Select(t => t.Id)
                .ToListAsync();

            // Single query for all messages (with Topic to get DiscussionId)
            var messagesByTopic = await _context.Messages
                .Where(m => topicIds.Contains(m.TopicId))
                .Include(m => m.Topic)
                .ToListAsync();

            // Group messages by discussion
            var messagesByDiscussion = messagesByTopic
                .GroupBy(m => m.Topic!.DiscussionId)
                .Select(g => new
                {
                    DiscussionId = g.Key,
                    MessageCount = g.Count(),
                    MessageUserIds = g.Select(m => m.AppUserId).Distinct().ToList(),
                    LastMessageTime = g.Max(m => (DateTime?)m.CreationTime)
                })
                .ToList();

            // Build DiscussionCardData list
            var discussionCards = discussions.Select(d =>
            {
                var topicData = topicsByDiscussion.FirstOrDefault(t => t.DiscussionId == d.Id);
                var messageData = messagesByDiscussion.FirstOrDefault(m => m.DiscussionId == d.Id);

                // Combine unique user IDs from topics and messages
                var allUserIds = new HashSet<Guid>();
                if (topicData != null)
                {
                    foreach (var uid in topicData.TopicUserIds)
                        allUserIds.Add(uid);
                }
                if (messageData != null)
                {
                    foreach (var uid in messageData.MessageUserIds)
                        allUserIds.Add(uid);
                }

                // Get last activity time
                var activityTimes = new List<DateTime?> { topicData?.LastTopicTime, messageData?.LastMessageTime };
                var lastActivity = activityTimes.Where(dt => dt.HasValue).OrderByDescending(dt => dt).FirstOrDefault();

                return new DiscussionCardData
                {
                    Id = d.Id,
                    Tittle = d.Tittle,
                    Description = d.Description,
                    CreationTime = d.CreationTime,
                    ImageData = d.imageData,
                    CreatorImageData = d.AppUser?.AvatarImageData,
                    CreatorUsername = d.AppUser?.UserName ?? "Unknown",
                    CreatorId = d.AppUserId,
                    BookTittle = d.Book?.Tittle,
                    BookId = d.BookId,
                    GenreName = d.Genre?.Name,
                    GenreId = d.GenreId,
                    AuthorName = d.Author?.Name,
                    AuthorId = d.AuthorId,
                    ParticipantsCount = allUserIds.Count,
                    TopicsCount = topicData?.TopicCount ?? 0,
                    MessagesCount = messageData?.MessageCount ?? 0,
                    LastActivityTime = lastActivity
                };
            }).ToList();

            // Step 5: Apply sorting
            var ascending = string.Equals(sortDirection, "ascending", StringComparison.OrdinalIgnoreCase);

            discussionCards = (sortBy?.ToLowerInvariant()) switch
            {
                "participants" => ascending
                    ? discussionCards.OrderBy(d => d.ParticipantsCount).ToList()
                    : discussionCards.OrderByDescending(d => d.ParticipantsCount).ToList(),

                "date" => ascending
                    ? discussionCards.OrderBy(d => d.CreationTime).ToList()
                    : discussionCards.OrderByDescending(d => d.CreationTime).ToList(),

                "activity" => ascending
                    ? discussionCards.OrderBy(d => d.LastActivityTime ?? DateTime.MinValue).ToList()
                    : discussionCards.OrderByDescending(d => d.LastActivityTime ?? DateTime.MaxValue).ToList(),

                _ => discussionCards.OrderByDescending(d => d.CreationTime).ToList() // Default: newest first
            };

            // Step 6: Build filter options from ALL discussions (not just filtered)
            var allDiscussions = await _context.Discussions
                .Include(d => d.Author)
                .Include(d => d.Book)
                .Include(d => d.Genre)
                .ToListAsync();

            var availableAuthors = allDiscussions
                .Where(d => d.Author != null)
                .Select(d => d.Author!)
                .DistinctBy(a => a.Id)
                .OrderBy(a => a.Name)
                .ToList();

            var availableGenres = allDiscussions
                .Where(d => d.Genre != null)
                .Select(d => d.Genre!)
                .DistinctBy(g => g.Id)
                .OrderBy(g => g.Name)
                .ToList();

            var availableBooks = allDiscussions
                .Where(d => d.Book != null)
                .Select(d => d.Book!)
                .DistinctBy(b => b.Id)
                .OrderBy(b => b.Tittle)
                .ToList();

            // Step 7: Build ViewModel
            var viewModel = new DiscussionIndexViewModel
            {
                Discussions = discussionCards,
                AvailableAuthors = availableAuthors,
                AvailableGenres = availableGenres,
                AvailableBooks = availableBooks,
                SearchInput = searchInput,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            // Step 8: Check if AJAX request
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase);

            if (isAjax)
                return PartialView("_DiscussionResults", viewModel);

            return View(viewModel);
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
