using BlogApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BlogContext _context;

        public AdminController(BlogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == "Pending")
                .OrderByDescending(p => p.PublishedOn)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.IsActive = true;
            post.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Yazı başarıyla onaylandı.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.IsActive = false;
            post.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Yazı reddedildi.";

            return RedirectToAction(nameof(Index));
        }
    }
}