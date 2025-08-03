using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace ContactService.Data;

public class ContactDbContext(DbContextOptions<ContactDbContext> options) : DbContext(options)
{
  public DbSet<Contact> Contacts => Set<Contact>();
  public DbSet<ContactInfo> ContactInfos => Set<ContactInfo>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Contact configuration
    modelBuilder.Entity<Contact>(entity =>
    {
      entity.HasKey(c => c.Id);
      entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
      entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
      entity.Property(c => c.Company).HasMaxLength(200);
      entity.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
      entity.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

      entity.HasMany(c => c.ContactInfos)
            .WithOne()
            .HasForeignKey(ci => ci.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ContactInfo configuration
    modelBuilder.Entity<ContactInfo>(entity =>
    {
      entity.HasKey(ci => ci.Id);
      entity.Property(ci => ci.InfoContent).IsRequired().HasMaxLength(500);
      entity.Property(ci => ci.InfoType).HasConversion<string>();
      entity.Property(ci => ci.CreatedAt).HasDefaultValueSql("NOW()");
    });
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    // Auto update timestamps
    var entries = ChangeTracker.Entries<Contact>()
        .Where(e => e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
      entry.Entity.UpdatedAt = DateTime.UtcNow;
    }

    return await base.SaveChangesAsync(cancellationToken);
  }
}