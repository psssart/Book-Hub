using System.Security.Claims;
using App.DAL.EF;
using App.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UsersController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            // Batch-query stats via GroupBy
            var ratingsCountByUser = await _context.Ratings
                .GroupBy(r => r.AppUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var discussionsCountByUser = await _context.Discussions
                .GroupBy(d => d.AppUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var topicsCountByUser = await _context.Topics
                .GroupBy(t => t.AppUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var messagesCountByUser = await _context.Messages
                .GroupBy(m => m.AppUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            // PurchasedBooks count per user via Purchase join
            var purchasedBooksCountByUser = await _context.PurchasedBooks
                .Include(pb => pb.Purchase)
                .GroupBy(pb => pb.Purchase!.AppUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var viewModels = new List<UserAdminViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModels.Add(new UserAdminViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    AvatarImageData = user.AvatarImageData,
                    Roles = roles.ToList(),
                    RatingsCount = ratingsCountByUser.GetValueOrDefault(user.Id),
                    DiscussionsCount = discussionsCountByUser.GetValueOrDefault(user.Id),
                    TopicsCount = topicsCountByUser.GetValueOrDefault(user.Id),
                    MessagesCount = messagesCountByUser.GetValueOrDefault(user.Id),
                    PurchasedBooksCount = purchasedBooksCountByUser.GetValueOrDefault(user.Id)
                });
            }

            return View(viewModels);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id.Value.ToString());
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                AvatarImageData = user.AvatarImageData,
                Roles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(viewModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserEditViewModel viewModel, IFormFile? avatarFile, List<string> selectedRoles)
        {
            if (id != viewModel.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            user.Email = viewModel.Email;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await avatarFile.CopyToAsync(memoryStream);
                user.AvatarImageData = memoryStream.ToArray();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                viewModel.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(viewModel);
            }

            // Role diff
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id.Value.ToString());
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new UserAdminViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                AvatarImageData = user.AvatarImageData,
                Roles = roles.ToList(),
                RatingsCount = await _context.Ratings.CountAsync(r => r.AppUserId == user.Id),
                DiscussionsCount = await _context.Discussions.CountAsync(d => d.AppUserId == user.Id),
                TopicsCount = await _context.Topics.CountAsync(t => t.AppUserId == user.Id),
                MessagesCount = await _context.Messages.CountAsync(m => m.AppUserId == user.Id),
                PurchasedBooksCount = await _context.PurchasedBooks
                    .Include(pb => pb.Purchase)
                    .CountAsync(pb => pb.Purchase!.AppUserId == user.Id)
            };

            return View(viewModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Self-deletion guard
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && Guid.Parse(currentUserId) == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return RedirectToAction(nameof(Index));

            // Explicitly delete related data
            var refreshTokens = await _context.RefreshTokens.Where(rt => rt.AppUserId == id).ToListAsync();
            _context.RefreshTokens.RemoveRange(refreshTokens);

            var subscriptions = await _context.UsersSubscriptions.Where(s => s.AppUserId == id).ToListAsync();
            _context.UsersSubscriptions.RemoveRange(subscriptions);

            var messages = await _context.Messages.Where(m => m.AppUserId == id).ToListAsync();
            _context.Messages.RemoveRange(messages);

            var topics = await _context.Topics.Where(t => t.AppUserId == id).ToListAsync();
            _context.Topics.RemoveRange(topics);

            var discussions = await _context.Discussions.Where(d => d.AppUserId == id).ToListAsync();
            _context.Discussions.RemoveRange(discussions);

            var ratings = await _context.Ratings.Where(r => r.AppUserId == id).ToListAsync();
            _context.Ratings.RemoveRange(ratings);

            var purchases = await _context.Purchases.Where(p => p.AppUserId == id).ToListAsync();
            var purchaseIds = purchases.Select(p => p.Id).ToList();
            var purchasedBooks = await _context.PurchasedBooks.Where(pb => purchaseIds.Contains(pb.PurchaseId)).ToListAsync();
            _context.PurchasedBooks.RemoveRange(purchasedBooks);
            _context.Purchases.RemoveRange(purchases);

            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
