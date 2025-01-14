using Afra_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Afra_App;

public class AfraAppContext : DbContext
{
    public DbSet<Person> People { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Role> Roles { get; set; }
    
    public DbSet<Otium> Otia { get; set; }
    public DbSet<OstiumInstallment> OtiaInstallments { get; set; }
    public DbSet<OtiumRegularity> OtiaRegularities { get; set; }
    public DbSet<OtiumsKategory> OtiaKategories { get; set; }
    public DbSet<OtiumEnrollment> OtiaEnrollments { get; set; }
    
    private readonly string _dbPath;
    
    public AfraAppContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = Path.Combine(path, "afra-app.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasOne(p => p.Mentor)
            .WithMany(p => p.Mentees);

        modelBuilder.Entity<Person>()
            .HasOne(p => p.Class)
            .WithMany(k => k.Students);

        modelBuilder.Entity<Person>()
            .HasMany(p => p.Roles)
            .WithMany();

        modelBuilder.Entity<Class>()
            .HasOne(k => k.Tutor)
            .WithMany(p => p.TutoredClasses);
        
        modelBuilder.Entity<Otium>()
            .HasOne(o => o.Kategory)
            .WithMany(k => k.Otia);
        
        modelBuilder.Entity<Otium>()
            .HasMany(o => o.Managers)
            .WithMany(p => p.ManagedOtia);
        
        modelBuilder.Entity<OstiumInstallment>()
            .HasOne(ot => ot.Otium)
            .WithMany(o => o.Installments);

        modelBuilder.Entity<OstiumInstallment>()
            .HasOne(ot => ot.Tutor)
            .WithMany();

        modelBuilder.Entity<OtiumRegularity>()
            .HasOne(or => or.Otium)
            .WithMany(o => o.Regularities);

        modelBuilder.Entity<OtiumRegularity>()
            .HasOne(or => or.Tutor)
            .WithMany();

        modelBuilder.Entity<OtiumRegularity>()
            .HasMany(or => or.Installments)
            .WithOne(or => or.Regularity)
            .OnDelete(DeleteBehavior.SetNull);
    }
}