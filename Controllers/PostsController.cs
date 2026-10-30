using BlogApp.ViewModels;
using BlogApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly BlogContext _context;

        public PostsController(BlogContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int? tagId)
        {
            var postsQuery = _context.Posts
                .Where(p => p.IsActive)
                .Include(p => p.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .AsQueryable();

            if (tagId.HasValue)
            {
                postsQuery = postsQuery
                    .Where(p => p.PostTags.Any(pt => pt.TagId == tagId.Value));
            }

            var posts = await postsQuery.ToListAsync();

            ViewBag.SelectedTagId = tagId;
            ViewBag.Tags = await _context.Tags.ToListAsync();

            return View(posts);
        }




        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreatePostViewModel();

            ViewBag.Tags = await _context.Tags.ToListAsync();

            return View(model);
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tags = await _context.Tags.ToListAsync();
                return View(model);
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                PublishedOn = DateTime.Now,
                IsActive = false,
                Status = "Pending",
                UserId = userId
            };

            foreach (var tagId in model.SelectedTagIds.Distinct())
            {
                var tag = await _context.Tags.FindAsync(tagId);

                if (tag != null)
                {
                    post.PostTags.Add(new PostTag
                    {
                        TagId = tag.Id,
                        Post = post
                    });
                }
            }

            _context.Posts.Add(post);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Yazın oluşturuldu. Admin onayından sonra yayınlanacak.";

            return RedirectToAction("Index");
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            if (post.UserId != userId)
            {
                return Forbid();
            }

            var model = new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                SelectedTagIds = post.PostTags.Select(pt => pt.TagId).ToList()
            };

            ViewBag.Tags = await _context.Tags.ToListAsync();

            return View(model);
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Tags = await _context.Tags.ToListAsync();
                return View(model);
            }

            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            if (post.UserId != userId)
            {
                return Forbid();
            }

            post.Title = model.Title;
            post.Content = model.Content;

            post.PostTags.Clear();

            foreach (var tagId in model.SelectedTagIds.Distinct())
            {
                var tagExists = await _context.Tags
                    .AnyAsync(t => t.Id == tagId);

                if (tagExists)
                {
                    post.PostTags.Add(new PostTag
                    {
                        PostId = post.Id,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = post.Id });
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin") && post.UserId != userId)
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Blog yazısı başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, CreateCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["CommentError"] = "Yorum boş bırakılamaz.";

                return RedirectToAction("Details", new { id = postId });
            }

            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var comment = new Comment
            {
                Text = model.Text,
                PublishedOn = DateTime.Now,
                UserId = userId,
                PostId = postId
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId });
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            int postId = comment.PostId;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId });
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            var model = new EditCommentViewModel
            {
                Text = comment.Text
            };

            ViewBag.CommentId = comment.Id;

            return View(model);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(
            int id, EditCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CommentId = id;
                return View(model);
            }

            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            comment.Text = model.Text;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details", new { id = comment.PostId });
        }
    }
}
