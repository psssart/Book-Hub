using Base.Tests.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Base.Tests.DAL;

public class TestDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public DbSet<TestEntity> TestEntities { get; set; } = default!;

    public TestDbContext(DbContextOptions options) : base(options)
    {
    }
}