using Afra_App.Data.Email;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Afra_App.Data;

/// <summary>
///     The database context for the Afra-App
/// </summary>
public class AfraAppContext : DbContext, IDataProtectionKeyContext
{
    /// <inheritdoc />
    public AfraAppContext(DbContextOptions<AfraAppContext> options) : base(options)
    {
    }

    /// <summary>
    ///     The DbSet for the people using the application.
    /// </summary>
    public DbSet<Person> Personen { get; set; }

    /// <summary>
    ///     All the Otia in the application.
    /// </summary>
    public DbSet<Otium.Otium> Otia { get; set; }

    /// <summary>
    ///     All instances of Otia
    /// </summary>
    public DbSet<Termin> OtiaTermine { get; set; }

    /// <summary>
    ///     All recurrence rules for Otia
    /// </summary>
    public DbSet<Wiederholung> OtiaWiederholungen { get; set; }

    /// <summary>
    ///     All categories for Otia
    /// </summary>
    public DbSet<Kategorie> OtiaKategorien { get; set; }

    /// <summary>
    ///     All enrollments for Otia
    /// </summary>
    public DbSet<Einschreibung> OtiaEinschreibungen { get; set; }

    /// <summary>
    ///     All school days
    /// </summary>
    public DbSet<Schultag> Schultage { get; set; }

    /// <summary>
    ///     All blocks on school days
    /// </summary>
    public DbSet<Block> Blocks { get; set; }

    /// <summary>
    ///     The Emails scheduled by the Application
    /// </summary>
    public DbSet<ScheduledEmail> ScheduledEmails { get; set; }

    /// <summary>
    ///     Configures the npgsql specific options for the context
    /// </summary>
    internal static Action<NpgsqlDbContextOptionsBuilder> ConfigureNpgsql =>
        builder => builder
            .MapEnum<Rolle>("person_rolle")
            .MapEnum<Wochentyp>("wochentyp");

    /// <summary>
    ///     The keys used by the ASP.NET Core Data Protection API.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasOne(p => p.Mentor)
            .WithMany(p => p.Mentees);

        modelBuilder.Entity<Otium.Otium>(o =>
        {
            o.HasOne(e => e.Kategorie)
                .WithMany(k => k.Otia);
            o.HasMany(e => e.Verantwortliche)
                .WithMany(p => p.VerwalteteOtia);
        });

        modelBuilder.Entity<Termin>(t =>
        {
            t.HasOne(ot => ot.Otium)
                .WithMany(o => o.Termine);
            t.HasOne(ot => ot.Tutor).WithMany();
        });

        modelBuilder.Entity<Wiederholung>(w =>
        {
            w.HasOne(or => or.Otium)
                .WithMany(o => o.Wiederholungen);
            w.HasOne(or => or.Tutor)
                .WithMany();
            w.HasMany(or => or.Termine)
                .WithOne(or => or.Wiederholung)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // record structs do not work with the [ComplexType] attribute.
        modelBuilder.Entity<Einschreibung>()
            .ComplexProperty(e => e.Interval);

        modelBuilder.Entity<ScheduledEmail>()
            .HasOne(e => e.Recipient);

        modelBuilder.Entity<Block>(b =>
        {
            b.HasOne(e => e.Schultag)
                .WithMany(e => e.Blocks)
                .HasForeignKey(e => e.SchultagKey);

            b.HasIndex(e => new { e.SchultagKey, Nummer = e.SchemaId })
                .IsUnique();
        });

        /*
         * This is a bit annoying, but we'll have to do it because of a bug in the Npgsql provider.
         * By default, it'll use '\0' as the default value for char columns, as it is the default value for char in C#.
         * However, the npgsql provider uses libpg for db access, which uses c strings, therefore thinks the string ends
         * at the first null character and fails.
         */
        modelBuilder.Entity<Block>()
            .Property(b => b.SchemaId)
            .HasDefaultValueSql("''");

        modelBuilder.Entity<Wiederholung>()
            .Property(w => w.Block)
            .HasDefaultValueSql("''");
    }
}
