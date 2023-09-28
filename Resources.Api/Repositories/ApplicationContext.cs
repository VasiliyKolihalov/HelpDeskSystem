using Microsoft.EntityFrameworkCore;
using Resources.Api.Models.Images;

namespace Resources.Api.Repositories;

public sealed class ApplicationContext : DbContext
{
    public DbSet<Image> Images { get; set; }
    public DbSet<ImageMessage> ImagesMessages { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}