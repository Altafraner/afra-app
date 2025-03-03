using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Data;

public class AfraAppContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<Person> Personen { get; set; }
    public DbSet<Otium.Otium> Otia { get; set; }
    public DbSet<Termin> OtiaTermine { get; set; }
    public DbSet<Wiederholung> OtiaWiederholungen { get; set; }
    public DbSet<Kategorie> OtiaKategorien { get; set; }
    public DbSet<Einschreibung> OtiaEinschreibungen { get; set; }
    public DbSet<Schultag> Schultage { get; set; }
    
    // This is used for the Data Protection API from .NET, used for example for securing auth cookies.
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    
    private readonly string _connectionString;
    
    public AfraAppContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseNpgsql(_connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasOne(p => p.Mentor)
            .WithMany(p => p.Mentees);
        
        modelBuilder.Entity<Otium.Otium>()
            .HasOne(o => o.Kategorie)
            .WithMany(k => k.Otia);
        
        modelBuilder.Entity<Otium.Otium>()
            .HasMany(o => o.Verantwortliche)
            .WithMany(p => p.VerwalteteOtia);
        
        modelBuilder.Entity<Termin>()
            .HasOne(ot => ot.Otium)
            .WithMany(o => o.Termine);

        modelBuilder.Entity<Termin>()
            .HasOne(ot => ot.Tutor)
            .WithMany();

        modelBuilder.Entity<Wiederholung>()
            .HasOne(or => or.Otium)
            .WithMany(o => o.Wiederholungen);

        modelBuilder.Entity<Wiederholung>()
            .HasOne(or => or.Tutor)
            .WithMany();

        modelBuilder.Entity<Wiederholung>()
            .HasMany(or => or.Termine)
            .WithOne(or => or.Wiederholung)
            .OnDelete(DeleteBehavior.SetNull);
    }
}