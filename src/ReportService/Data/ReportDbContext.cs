using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace ReportService.Data;

public class ReportDbContext(DbContextOptions<ReportDbContext> options) : DbContext(options)
{
  public DbSet<Report> Reports => Set<Report>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Report>(entity =>
    {
      entity.HasKey(r => r.Id);
      entity.Property(r => r.RequestedDate).HasDefaultValueSql("NOW()");
      entity.Property(r => r.Status).HasConversion<string>();

      // Statistics JSON olarak saklanabilir
      entity.OwnsMany(r => r.Statistics, s =>
      {
        s.Property(stat => stat.Location).IsRequired().HasMaxLength(200);
        s.Property(stat => stat.PersonCount);
        s.Property(stat => stat.PhoneNumberCount);
      });
    });
  }
}