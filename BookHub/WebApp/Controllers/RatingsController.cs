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
    public class RatingsController : Controller
    {
        private readonly IAppBLL _bll;
        /*private readonly IAppUnitOfWork _uow;*/

        public RatingsController(IAppBLL bll)
        {
            _bll = bll;
            /*_uow = new AppUOW(context);*/
        }

        // GET: Ratings
        public async Task<IActionResult> Index()
        {
            var res = await _bll.Ratings.GetAllAsync();
            return View(res);
        }

        // GET: Ratings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var rating = await _bll.Ratings.FirstOrDefaultAsync(id.Value);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // GET: Ratings/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id");
            return View();
        }

        // POST: Ratings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppUserId,BookId,Value,Comment,Id")] App.BLL.DTO.Rating rating)
        {
            if (ModelState.IsValid)
            {
                rating.Id = Guid.NewGuid();
                _bll.Ratings.Add(rating);
                await _bll.SaveChangesAsync();
                return RedirectToAction("Index", "PurchasedBooks");
            }
            ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", rating.AppUserId);
            return RedirectToAction("Index", "PurchasedBooks");
        }

        // GET: Ratings/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var rating = await _bll.Ratings.FirstOrDefaultIncludeAllAsync(id.Value);
            if (rating == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = rating.AppUserId;
            ViewData["BookId"] = rating.BookId;
            /*ViewData["AppUserId"] = new SelectList(_bll.Users.GetAll(), "Id", "Id", rating.AppUserId);*/
            return View(rating);
        }

        // POST: Ratings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AppUserId,BookId,Value,Comment,Id")] App.BLL.DTO.Rating rating)
        {
            if (id != rating.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _bll.Ratings.Update(rating);
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await _bll.Ratings.ExistsAsync(rating.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "PurchasedBooks");
            }
            
            return RedirectToAction("Index", "PurchasedBooks");
        }

        // GET: Ratings/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _bll.Ratings
                .FirstOrDefaultAsync(id.Value);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var rating = await _bll.Ratings.ExistsAsync(id);
            if (rating)
            {
                await _bll.Ratings.RemoveAsync(id);
            }

            await _bll.SaveChangesAsync();
            return RedirectToAction("Index", "PurchasedBooks");
        }
    }
}
