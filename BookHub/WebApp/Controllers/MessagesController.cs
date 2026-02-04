using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Contracts.BLL;
using App.Contracts.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Entities;
using WebApp.Hubs;
using WebApp.Models.SignalR;

namespace WebApp.Controllers
{
    public class MessagesController : Controller
    {
        private readonly IAppBLL _bll;
        private readonly IHubContext<DiscussionHub> _hubContext;

        public MessagesController(IAppBLL bll, IHubContext<DiscussionHub> hubContext)
        {
            _bll = bll;
            _hubContext = hubContext;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            var messages = await _bll.Messages.GetAllIncludeAll();
            return View(messages.ToList());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var message = await _bll.Messages.FirstOrDefaultIncludeAllAsync(id.Value);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id");
            ViewData["TopicId"] = new SelectList(_bll.Topics.GetAll(), "Id", "Content");
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TopicId,AppUserId,Content,Id")] App.BLL.DTO.Message message)
        {
            if (ModelState.IsValid)
            {
                message.Id = Guid.NewGuid();
                message.CreationTime = DateTime.UtcNow;
                _bll.Messages.Add(message);
                await _bll.SaveChangesAsync();

                // Fetch the complete message with user info for broadcasting
                var savedMessage = await _bll.Messages.FirstOrDefaultIncludeAllAsync(message.Id);

                if (savedMessage != null)
                {
                    // Broadcast to all clients in this topic's room
                    var notification = new NewMessageNotification
                    {
                        MessageId = savedMessage.Id,
                        TopicId = savedMessage.TopicId,
                        Content = savedMessage.Content,
                        UserName = savedMessage.AppUser?.UserName ?? "Unknown",
                        CreationTime = savedMessage.CreationTime
                    };

                    await _hubContext.Clients
                        .Group($"topic_{message.TopicId}")
                        .SendAsync("ReceiveMessage", notification);
                }

                return RedirectToAction("Details", "Topics", new { id = message.TopicId });
            }
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", message.AppUserId);
            ViewData["TopicId"] = new SelectList(_bll.Topics.GetAll(), "Id", "Content", message.TopicId);
            return RedirectToAction("Details", "Topics", new { id = message.TopicId });
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _bll.Messages.FirstOrDefaultIncludeAllAsync(id.Value);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", message.AppUserId);
            ViewData["TopicId"] = new SelectList(_bll.Topics.GetAll(), "Id", "Content", message.TopicId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("TopicId,AppUserId,Content,Id")] App.BLL.DTO.Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _bll.Messages.Update(message);
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await _bll.Messages.ExistsAsync(message.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Get the topic to find the discussion ID for redirect
                var topic = await _bll.Topics.FirstOrDefaultAsync(message.TopicId);
                return RedirectToAction("Details", "Discussions", new { id = topic?.DiscussionId });
            }

            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", message.AppUserId);
            ViewData["TopicId"] = new SelectList(_bll.Topics.GetAll(), "Id", "Content", message.TopicId);
            // Re-fetch with includes for view context
            var messageWithIncludes = await _bll.Messages.FirstOrDefaultIncludeAllAsync(message.Id);
            return View(messageWithIncludes ?? message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _bll.Messages.FirstOrDefaultAsync(id.Value);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var message = await _bll.Messages.ExistsAsync(id);
            if (message)
            {
                await _bll.Messages.RemoveAsync(id);
            }

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
