using Microsoft.EntityFrameworkCore;
using Users.Api.Models.Users;

namespace Users.Api.Repositories;

public sealed class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
}