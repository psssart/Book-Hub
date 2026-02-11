using System.Text.Json;
using App.DAL.EF;
using App.Domain.Address_Tables;
using App.Domain.Entities;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WebApp.Infrastructure.Data.SeedDTO;

namespace WebApp.Infrastructure.Data;

/// <summary>
/// Primary interactions with a database
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seed all requested data from config
    /// </summary>
    /// <param name="app"></param>
    public static async Task SeedAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        if (!context.Database.ProviderName!.Contains("InMemory"))
        {
            await context.Database.MigrateAsync();
        }

        var seedSettings = config.GetSection("SeedData").Get<SeedSettings>() ?? new SeedSettings();

        await SeedUsersAsync(seedSettings.Users, userManager, roleManager);
        await SeedPublishersAsync(context, seedSettings.Publishers);
        await SeedAuthorsAsync(context, seedSettings.Authors);
        if (seedSettings.Genres) await SeedGenresAsync(context);
        await SeedWarehousesAsync(context, seedSettings.Warehouses);
        await SeedBooksAsync(context, seedSettings.Books);
    }
    
        private static async Task SeedUsersAsync(
        List<UserSeedDto> users,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        foreach (var u in users)
        {
            foreach (var role in u.Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole { Name = role });
                }
            }

            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing != null) continue;

            var user = new AppUser
            {
                Email = u.Email,
                UserName = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName
            };

            var createRes = await userManager.CreateAsync(user, u.Password);
            if (!createRes.Succeeded)
            {
                Console.WriteLine($"Error creating {u.Email}: {string.Join(", ", createRes.Errors.Select(e => e.Description))}");
                continue;
            }

            var addRoleRes = await userManager.AddToRolesAsync(user, u.Roles);
            if (!addRoleRes.Succeeded)
            {
                Console.WriteLine($"Error assigning roles to {u.Email}: {string.Join(", ", addRoleRes.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedPublishersAsync(AppDbContext context, int count)
    {
        if (context.Publishers.Any()) return;

        var data = await File.ReadAllTextAsync(GetSeedFilePath("publishers.json"));
        var publishers = JsonSerializer.Deserialize<List<PublisherSeedDto>>(data) ?? new();

        foreach (var publisher in publishers.Take(count))
        {
            context.Publishers.Add(new Publisher
            {
                Name = publisher.Name,
                Description = publisher.Description
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAuthorsAsync(AppDbContext context, int count)
    {
        if (context.Authors.Any()) return;

        var data = await File.ReadAllTextAsync(GetSeedFilePath("authors.json"));
        var authors = JsonSerializer.Deserialize<List<AuthorSeedDto>>(data) ?? new();

        foreach (var author in authors.Take(count))
        {
            var imageName = author.FullName.ToLower().Replace(" ", "-") + ".jpg";
            var imagePath = Path.Combine("wwwroot", "img", "authors", imageName);
            byte[]? imageData = File.Exists(imagePath) ? await File.ReadAllBytesAsync(imagePath) : null;

            context.Authors.Add(new Author
            {
                Name = author.FullName,
                Age = DateTime.UtcNow.Year - author.BirthYear,
                Biography = author.Biography,
                imageData = imageData
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedGenresAsync(AppDbContext context)
    {
        if (context.Genres.Any()) return;

        var data = await File.ReadAllTextAsync(GetSeedFilePath("genres.json"));
        var genres = JsonSerializer.Deserialize<List<GenreSeedDto>>(data) ?? new();

        context.Genres.AddRange(genres.Select(g => new Genre
        {
            Name = g.Name,
            Description = g.Description,
            IsMainGenre = g.IsMainGenre
        }));

        await context.SaveChangesAsync();
    }

    private static async Task SeedWarehousesAsync(AppDbContext context, int count)
    {
        if (context.Warehouses.Any()) return;

        var data = await File.ReadAllTextAsync(GetSeedFilePath("warehouses.json"));
        var warehouses = JsonSerializer.Deserialize<List<WarehouseSeedDto>>(data) ?? new();

        foreach (var w in warehouses.Take(count))
        {
            context.Warehouses.Add(new Warehouse
            {
                Name = w.Name,
                GpsX = w.GpsX,
                GpsY = w.GpsY,
                Country = w.Country,
                Location = new Point(w.GpsY, w.GpsX) { SRID = 4326 }
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedBooksAsync(AppDbContext context, int count)
    {
        if (context.Books.Any()) return;

        var data = await File.ReadAllTextAsync(GetSeedFilePath("books.json"));
        var books = JsonSerializer.Deserialize<List<BookSeedDto>>(data) ?? new();
        var random = new Random();

        foreach (var bookDto in books.Take(count))
        {
            var publisher = context.Publishers.FirstOrDefault(p => p.Name == bookDto.Publisher);
            if (publisher == null) continue;

            var imageName = bookDto.Title.ToLower().Replace(" ", "-") + ".jpg";
            var imagePath = Path.Combine("wwwroot", "img", "books", imageName);
            byte[]? imageData = File.Exists(imagePath) ? await File.ReadAllBytesAsync(imagePath) : null;

            var book = new Book
            {
                Tittle = bookDto.Title,
                Description = bookDto.Description,
                ReleaseYear = bookDto.ReleaseYear,
                Price = bookDto.Price,
                PublisherId = publisher.Id,
                imageData = imageData
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();

            // Link Authors
            foreach (var authorName in bookDto.AuthorNames)
            {
                var author = context.Authors.FirstOrDefault(a => a.Name == authorName);
                if (author != null)
                {
                    context.BooksAuthors.Add(new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = author.Id
                    });
                }
            }

            // Link Genres
            foreach (var genreName in bookDto.Genres)
            {
                var genre = context.Genres.FirstOrDefault(g => g.Name == genreName);
                if (genre != null)
                {
                    context.BooksGenres.Add(new BookGenre
                    {
                        BookId = book.Id,
                        GenreId = genre.Id
                    });
                }
            }

            // Link Warehouses (1 to 3 random)
            var warehouseIds = context.Warehouses.Select(w => w.Id).ToList();
            var selected = warehouseIds.OrderBy(_ => random.Next()).Take(random.Next(1, 4));

            foreach (var warehouseId in selected)
            {
                context.BooksWarehouses.Add(new BookWarehouses
                {
                    BookId = book.Id,
                    WarehouseId = warehouseId,
                    Count = random.Next(1, 16),
                    LastSupply = DateTime.UtcNow.AddDays(-random.Next(0, 91))
                });
            }

            await context.SaveChangesAsync();
        }
    }
    
    private static string GetSeedFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Data", "SeedData", fileName);
    }
}