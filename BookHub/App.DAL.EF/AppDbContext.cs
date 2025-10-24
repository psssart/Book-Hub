using App.Domain.Address_Tables;
using App.Domain.Entities;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole,
    IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>

{
    public DbSet<AppRefreshToken> RefreshTokens { get; set; } = default!;
    
    public DbSet<Publisher> Publishers { get; set; } = default!;
    public DbSet<Book> Books { get; set; } = default!;
    public DbSet<Warehouse> Warehouses { get; set; } = default!;
    public DbSet<BookWarehouses> BooksWarehouses { get; set; } = default!;
    public DbSet<Genre> Genres { get; set; } = default!;
    public DbSet<BookGenre> BooksGenres { get; set; } = default!;
    public DbSet<Author> Authors { get; set; } = default!;
    public DbSet<BookAuthor> BooksAuthors { get; set; } = default!;
    public DbSet<Rating> Ratings { get; set; } = default!;
    public DbSet<UserSubscription> UsersSubscriptions { get; set; } = default!;
    public DbSet<Purchase> Purchases { get; set; } = default!;
    public DbSet<PurchasedBook> PurchasedBooks { get; set; } = default!;
    public DbSet<Discussion> Discussions { get; set; } = default!;
    public DbSet<Topic> Topics { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;
    
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entity in ChangeTracker.Entries().Where(e => e.State != EntityState.Deleted))
        {
            foreach (var prop in entity
                         .Properties
                         .Where(x => x.Metadata.ClrType == typeof(DateTime)))
            {
                Console.WriteLine(prop);
                prop.CurrentValue = ((DateTime) prop.CurrentValue).ToUniversalTime();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Book>(b =>
        {
            b.Property(x => x.SearchVector)
                .HasColumnName("search_vector")
                .HasColumnType("tsvector");

            b.HasIndex(x => x.SearchVector)
                .HasMethod("GIN")
                .HasDatabaseName("idx_books_search_vector");

            b.HasIndex(x => x.Tittle)
                .HasMethod("GIN")
                .HasDatabaseName("idx_books_title_trgm")
                .HasOperators("gin_trgm_ops");
        });

        builder.Entity<Author>(a =>
        {
            a.Property(x => x.SearchVector)
                .HasColumnName("search_vector")
                .HasColumnType("tsvector");

            a.HasIndex(x => x.SearchVector)
                .HasMethod("GIN")
                .HasDatabaseName("idx_authors_search_vector");

            a.HasIndex(x => x.Name)
                .HasMethod("GIN")
                .HasDatabaseName("idx_authors_name_trgm")
                .HasOperators("gin_trgm_ops");
        });
    }
    
}
