
using AuthDemo.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Configure cookie authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Create the data folder and users.json file if needed
var dataDirectory = Path.Combine(app.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDirectory);

var usersFile = Path.Combine(dataDirectory, "users.json");

// Load existing users
List<User> users = new List<User>();

if (File.Exists(usersFile))
{
    var json = File.ReadAllText(usersFile);
    users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
}

// Create the admin account if no users exist
if (users.Count == 0)
{
    var admin = new User
    {
        Username = "admin"
    };

    var passwordHasher = new PasswordHasher<User>();
    admin.PasswordHash = passwordHasher.HashPassword(admin, "Password123");

    users.Add(admin);

    var options = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    File.WriteAllText(usersFile, JsonSerializer.Serialize(users, options));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
