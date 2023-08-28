using Microsoft.EntityFrameworkCore;
using Resources.WebApi.Models.Images;

namespace Resources.WebApi.Repositories;

public sealed class ApplicationContext : DbContext
{
    public DbSet<Image> Images { get; set; }
    public DbSet<ImageMessage> ImagesMessages { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}