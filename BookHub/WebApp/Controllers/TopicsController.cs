using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Contracts.BLL;
using App.Contracts.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Entities;

namespace WebApp.Controllers
{
    public class TopicsController : Controller
    {
        private readonly IAppBLL _bll;
        /*private readonly IAppUnitOfWork _uow;*/

        public TopicsController(IAppBLL bll)
        {
            _bll = bll;
            /*_uow = new AppUOW(context);*/
        }

        // GET: Topics
        public async Task<IActionResult> Index()
        {
            var topics = await _bll.Topics.GetAllIncludeAll();
            return View(topics.ToList());
        }

        // GET: Topics/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _bll.Topics.FirstOrDefaultIncludeAllAsync(id.Value);
            if (topic == null)
            {
                return NotFound();
            }

            var messages = await _bll.Messages.GetAllBy1TopicIncludeUserAsync(id.Value);

            // Pass the messages to the view
            ViewData["Messages"] = messages.ToList();
            
            return View(topic);
        }

        // GET: Topics/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Topics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppUserId,DiscussionId,Tittle,Content,Id")] App.BLL.DTO.Topic topic)
        {
            if (ModelState.IsValid)
            {
                topic.Id = Guid.NewGuid();
                topic.CreationTime = DateTime.UtcNow;
                _bll.Topics.Add(topic);
                await _bll.SaveChangesAsync();
                return RedirectToAction("Details", "Discussions", new { id = topic.DiscussionId });
            }
            return RedirectToAction("Details", "Discussions", new { id = topic.DiscussionId });
        }

        // GET: Topics/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _bll.Topics.FirstOrDefaultAsync(id.Value);
            if (topic == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", topic.AppUserId);
            // ViewData["DiscussionId"] = new SelectList(_context.Discussions, "Id", "Tittle", topic.DiscussionId);
            return View(topic);
        }

        // POST: Topics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AppUserId,DiscussionId,Tittle,Content,Id")] App.BLL.DTO.Topic topic)
        {
            if (id != topic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _bll.Topics.Update(topic);
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await _bll.Topics.ExistsAsync(topic.Id))
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
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", topic.AppUserId);
            return View(topic);
        }

        // GET: Topics/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _bll.Topics.FirstOrDefaultIncludeAllAsync(id.Value);
            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }

        // POST: Topics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var topic = await _bll.Topics.ExistsAsync(id);
            if (topic)
            {
                await _bll.Topics.RemoveAsync(id);
            }

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
