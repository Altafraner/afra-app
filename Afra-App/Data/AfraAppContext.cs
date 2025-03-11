using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Data;

/// <summary>
/// The database context for the Afra-App
/// </summary>
public class AfraAppContext : DbContext, IDataProtectionKeyContext
{
    /// <summary>
    /// The DbSet for the people using the application.
    /// </summary>
    public DbSet<Person> Personen { get; set; }
    
    /// <summary>
    /// All the Otia in the application.
    /// </summary>
    public DbSet<Otium.Otium> Otia { get; set; }
    
    /// <summary>
    /// All instances of Otia
    /// </summary>
    public DbSet<Termin> OtiaTermine { get; set; }
    
    /// <summary>
    /// All recurrence rules for Otia
    /// </summary>
    public DbSet<Wiederholung> OtiaWiederholungen { get; set; }
    
    /// <summary>
    /// All categories for Otia
    /// </summary>
    public DbSet<Kategorie> OtiaKategorien { get; set; }
    
    /// <summary>
    /// All enrollments for Otia
    /// </summary>
    public DbSet<Einschreibung> OtiaEinschreibungen { get; set; }
    
    /// <summary>
    /// All school days
    /// </summary>
    public DbSet<Schultag> Schultage { get; set; }
    
    /// <summary>
    /// The keys used by the ASP.NET Core Data Protection API.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    /// <inheritdoc />
    public AfraAppContext(DbContextOptions<AfraAppContext> options) : base(options)
    {
    }

    /// <inheritdoc />
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

        // Have to do this here because the [ComplexType] annotation is not valid on record structs.
        modelBuilder.Entity<Einschreibung>()
            .ComplexProperty(e => e.Interval);
    }
}