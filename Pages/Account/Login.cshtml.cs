using AuthDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
namespace AuthDemo.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public LoginModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [BindProperty]
    [Required]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Password { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var usersFile = Path.Combine(
            _environment.ContentRootPath,
            "data",
            "users.json");

        var json = await System.IO.File.ReadAllTextAsync(usersFile);

        var users = JsonSerializer.Deserialize<List<User>>(json)
                    ?? new List<User>();

        var user = users.FirstOrDefault(u =>
            u.Username.Equals(Username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var passwordHasher = new PasswordHasher<User>();

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            Password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        return RedirectToPage("/Admin/Index");
    }
}
