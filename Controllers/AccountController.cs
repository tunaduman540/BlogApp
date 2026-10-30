
using BlogApp.Data;
using BlogApp.Entities;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BlogApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly BlogContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(BlogContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool emailExists = _context.Users.Any(
                u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Bu e-posta adresi zaten kayıtlı.");

                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email
            };

            user.Password = _passwordHasher.HashPassword(
                user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(model);
            }

            PasswordVerificationResult result;

            try
            {
                result = _passwordHasher.VerifyHashedPassword(
                    user, user.Password, model.Password);
            }
            catch (FormatException)
            {
                ModelState.AddModelError(
                    "",
                    "Bu hesabın şifresi eski formatta. Lütfen yeni bir hesap oluşturun veya yöneticiyle iletişime geçin.");

                return View(model);
            }

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Posts");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var user = await _context.Users
                .Include(u => u.Posts)
                .ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Posts = user.Posts
        .OrderByDescending(p => p.PublishedOn)
        .ToList()
            };
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> UserProfile(int id)
        {
            var user = await _context.Users
                .Include(u => u.Posts)
                .ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Posts = user.Posts
        .OrderByDescending(p => p.PublishedOn)
        .ToList()
            };

            return View("Profile", model);
        }


        [Authorize]
        [HttpGet]
        public IActionResult EditProfileImage()
        {
            return View(new ProfileImageViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileImage(ProfileImageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var file = model.ProfileImage!;

            // Dosya boyutunu en fazla 2 MB ile sınırla.
            if (file.Length == 0 || file.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.ProfileImage),
                    "Fotoğraf boş olamaz ve en fazla 2 MB olmalıdır.");

                return View(model);
            }

            // Yalnızca JPEG, PNG ve WebP dosyalarını kabul et.
            var allowedTypes = new[]
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError(
                    nameof(model.ProfileImage),
                    "Yalnızca JPEG, PNG veya WebP fotoğrafları yükleyebilirsiniz.");

                return View(model);
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            user.ProfileImage = memoryStream.ToArray();
            user.ProfileImageContentType = file.ContentType;

            await _context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }


        [HttpGet]
        public async Task<IActionResult> ProfileImage(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.ProfileImage == null)
            {
                return NotFound();
            }

            return File(
                user.ProfileImage,
                user.ProfileImageContentType ?? "image/jpeg"
            );
        }

    }
}
