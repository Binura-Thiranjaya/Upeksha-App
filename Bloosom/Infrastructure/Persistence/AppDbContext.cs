// ...existing code...
using Bloosom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderAddOn> OrderAddOns { get; set; } = null!;
    public DbSet<Testimonial> Testimonials { get; set; } = null!;
    public DbSet<EventCategory> EventCategories { get; set; } = null!;
    public DbSet<EventImage> EventImages { get; set; } = null!;
    public DbSet<FacebookEmbed> FacebookEmbeds { get; set; } = null!;
    public DbSet<SiteSetting> SiteSettings { get; set; } = null!;
    public DbSet<ActivityEntry> ActivityEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).IsRequired();
            b.Property(x => x.FullName).IsRequired();
        });

        modelBuilder.Entity<SiteSetting>(b =>
        {
            b.HasKey(x => x.Key);
            b.Property(x => x.Value).IsRequired();
            b.Property(x => x.Type).IsRequired();
        });

        modelBuilder.Entity<ActivityEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).IsRequired();
            b.Property(x => x.Section).IsRequired();
        });

        modelBuilder.Entity<Category>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired();
            b.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<OrderAddOn>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Order).WithMany(o => o.AddOns).HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<EventCategory>(b =>
        {
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<EventImage>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.EventCategory).WithMany(e => e.Images).HasForeignKey(x => x.EventCategoryId);
        });
    }
}

// ...existing code...
