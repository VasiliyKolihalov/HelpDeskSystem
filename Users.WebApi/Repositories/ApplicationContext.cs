using Microsoft.EntityFrameworkCore;
using Users.WebApi.Models.Users;

namespace Users.WebApi.Repositories;

public sealed class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
}