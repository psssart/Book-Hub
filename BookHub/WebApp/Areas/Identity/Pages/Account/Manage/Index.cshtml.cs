#nullable disable
using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public IndexModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
        [TempData] public string StatusMessage { get; set; }
        [BindProperty] public InputModel Input { get; set; }
        
        public string AvatarUrl { get; private set; } = "#";

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Avatar")]
            [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "Max 2 MB")]
            [AllowedImageExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" })]
            public IFormFile Avatar { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            Input = new InputModel { PhoneNumber = phoneNumber };

            AvatarUrl = user.AvatarImageData != null
                ? Url.Page("./Index", pageHandler: "Avatar", values: new { v = DateTimeOffset.UtcNow.ToUnixTimeSeconds() })
                : "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iOTYiIGhlaWdodD0iOTYiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0iI2FkYjViZCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIxMiIgY3k9IjgiIHI9IjQiIC8+PHBhdGggZD0iTTE5LjUgMjBjMC0yLjktNC01LTcuNS01UzQuNSAxNy4xIDQuNSAyMGgxNSIgLz48L3N2Zz4=";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // phone
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // avatar upload if provided
            if (Input.Avatar != null && Input.Avatar.Length > 0)
            {
                using var ms = new MemoryStream();
                await Input.Avatar.CopyToAsync(ms);
                var bytes = ms.ToArray();

                // quick content sniff to reject non-images
                var mime = DetectImageMime(bytes);
                if (mime == null)
                {
                    ModelState.AddModelError("Input.Avatar", "Unsupported or invalid image.");
                    await LoadAsync(user);
                    return Page();
                }

                user.AvatarImageData = bytes;
                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    StatusMessage = "Unexpected error while saving avatar.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            
            AvatarUrl = Url.Page("./Index", pageHandler: "Avatar", values: new { v = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
            return RedirectToPage();
        }

        // POST handler for Remove button
        public async Task<IActionResult> OnPostRemoveAvatarAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            user.AvatarImageData = null;
            var update = await _userManager.UpdateAsync(user);
            StatusMessage = update.Succeeded ? "Photo removed." : "Failed to remove photo.";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }

        // GET /Manage/Index?handler=Avatar
        public async Task<IActionResult> OnGetAvatarAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AvatarImageData == null) return NotFound();

            var mime = DetectImageMime(user.AvatarImageData) ?? "image/jpeg";
            return File(user.AvatarImageData, mime);
        }

        // naive magic-number detection
        private static string DetectImageMime(byte[] data)
        {
            if (data.Length >= 4)
            {
                // PNG
                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47) return "image/png";
                // JPEG
                if (data[0] == 0xFF && data[1] == 0xD8) return "image/jpeg";
                // GIF
                if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46) return "image/gif";
                // WEBP (RIFF....WEBP)
                if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 && data.Length >= 12 &&
                    data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50) return "image/webp";
            }
            return null;
        }
    }

    // Validation attributes
    public sealed class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxBytes;
        public MaxFileSizeAttribute(int maxBytes) => _maxBytes = maxBytes;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile f && f.Length > _maxBytes)
                return new ValidationResult(ErrorMessage ?? $"File too large (>{_maxBytes} bytes).");
            return ValidationResult.Success;
        }
    }

    public sealed class AllowedImageExtensionsAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _exts;
        public AllowedImageExtensionsAttribute(string[] extensions)
        {
            _exts = extensions.Select(e => e.ToLowerInvariant()).ToHashSet();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile f)
            {
                var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant() ?? "";
                if (!_exts.Contains(ext))
                    return new ValidationResult(ErrorMessage ?? $"Invalid file type: {ext}");
            }
            return ValidationResult.Success;
        }
    }
}
