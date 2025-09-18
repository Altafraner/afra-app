using Afra_App.Backbone.Email.Domain.Models;
using Afra_App.Calendar.Domain.Models;
using Afra_App.Otium.Domain.Models;
using Afra_App.Profundum.Domain.Models;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.User.Domain.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Afra_App;

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
    ///     Contains the relations between mentors and mentees.
    /// </summary>
    public DbSet<MentorMenteeRelation> MentorMenteeRelations { get; set; }

    /// <summary>
    ///     All the Otia in the application.
    /// </summary>
    public DbSet<OtiumDefinition> Otia { get; set; }

    /// <summary>
    ///     All instances of Otia
    /// </summary>
    public DbSet<OtiumTermin> OtiaTermine { get; set; }

    /// <summary>
    ///     All recurrence rules for Otia
    /// </summary>
    public DbSet<OtiumWiederholung> OtiaWiederholungen { get; set; }

    /// <summary>
    ///     All categories for Otia
    /// </summary>
    public DbSet<OtiumKategorie> OtiaKategorien { get; set; }

    /// <summary>
    ///     All enrollments for Otia
    /// </summary>
    public DbSet<OtiumEinschreibung> OtiaEinschreibungen { get; set; }

    /// <summary>
    ///     All attendances for Otia
    /// </summary>
    public DbSet<OtiumAnwesenheit> OtiaAnwesenheiten { get; set; }

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
    ///     All registered Profunda
    /// </summary>
    public DbSet<ProfundumDefinition> Profunda { get; set; }

    /// <summary>
    ///     The slot instances op all registered Profunda
    /// </summary>
    public DbSet<ProfundumInstanz> ProfundaInstanzen { get; set; }

    /// <summary>
    ///     All finally matched Enrollments for Profunda
    /// </summary>
    public DbSet<ProfundumEinschreibung> ProfundaEinschreibungen { get; set; }

    /// <summary>
    ///     All enrollment wishes for produnda submitted by students
    /// </summary>
    public DbSet<ProfundumBelegWunsch> ProfundaBelegWuensche { get; set; }

    /// <summary>
    ///     All slots for profunda to have ProfundaInstanzen in
    /// </summary>
    public DbSet<ProfundumSlot> ProfundaSlots { get; set; }

    /// <summary>
    ///     All Einwahlzeiträume for Profundum
    /// </summary>
    public DbSet<ProfundumEinwahlZeitraum> ProfundumEinwahlZeitraeume { get; set; }

    /// <summary>
    ///     All Kategorien for Profunda
    /// </summary>
    public DbSet<ProfundumKategorie> ProfundaKategorien { get; set; }

    /// <summary>
    ///    All Bewertungskriterien for Profunda
    /// </summary>
    public DbSet<ProfundumBewertungKriterium> ProfundumBewertungKriterien { get; set; }

    /// <summary>
    ///    All Bewertungen for Profunda
    /// </summary>
    public DbSet<ProfundumBewertung> ProfundumBewertungen { get; set; }

    /// <summary>
    ///     All Calendar Subscriptions
    /// </summary>
    public DbSet<CalendarSubscription> CalendarSubscriptions { get; set; }

    /// <summary>
    ///     Configures the npgsql specific options for the context
    /// </summary>
    internal static Action<NpgsqlDbContextOptionsBuilder> ConfigureNpgsql =>
        builder => builder
            .MapEnum<Rolle>("person_rolle")
            .MapEnum<GlobalPermission>("global_permission")
            .MapEnum<Wochentyp>("wochentyp")
            .MapEnum<OtiumAnwesenheitsStatus>("anwesenheits_status");

    /// <summary>
    ///     The keys used by the ASP.NET Core Domain Protection API.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasMany(p => p.Mentors)
            .WithMany(p => p.Mentees)
            .UsingEntity<MentorMenteeRelation>(
                r => r.HasOne<Person>().WithMany().HasForeignKey(e => e.MentorId),
                l => l.HasOne<Person>().WithMany().HasForeignKey(e => e.StudentId));

        modelBuilder.Entity<Person>()
            .PrimitiveCollection(p => p.GlobalPermissions);

        modelBuilder.Entity<MentorMenteeRelation>()
            .HasKey(r => new { r.MentorId, r.StudentId });

        modelBuilder.Entity<OtiumDefinition>(o =>
        {
            o.HasOne(e => e.Kategorie)
                .WithMany(k => k.Otia);
            o.HasMany(e => e.Verantwortliche)
                .WithMany(p => p.VerwalteteOtia);
        });

        modelBuilder.Entity<OtiumTermin>(t =>
        {
            t.HasOne(ot => ot.Otium)
                .WithMany(o => o.Termine);
            t.HasOne(ot => ot.Tutor).WithMany();
            t.HasOne(ot => ot.Block).WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OtiumWiederholung>(w =>
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
        modelBuilder.Entity<OtiumEinschreibung>()
            .ComplexProperty(e => e.Interval);

        modelBuilder.Entity<OtiumAnwesenheit>(e =>
        {
            e.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId);

            e.HasOne(a => a.Block)
                .WithMany()
                .HasForeignKey(a => a.BlockId);

            e.HasKey(a => new { a.BlockId, a.StudentId });
        });

        modelBuilder.Entity<ScheduledEmail>()
            .HasOne(e => e.Recipient)
            .WithMany()
            .HasForeignKey(e => e.RecipientId);

        modelBuilder.Entity<Block>(b =>
        {
            b.HasOne(e => e.Schultag)
                .WithMany(e => e.Blocks)
                .HasForeignKey(e => e.SchultagKey)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(e => new { e.SchultagKey, Nummer = e.SchemaId })
                .IsUnique();
        });

        modelBuilder.Entity<ProfundumDefinition>(p =>
        {
            p.HasOne(p => p.Kategorie)
                .WithMany(k => k.Profunda);
        });
        modelBuilder.Entity<ProfundumInstanz>(p =>
        {
            p.HasOne(i => i.Profundum)
                .WithMany(p => p.Instanzen);
            p.HasMany(p => p.Slots).WithMany();
        });
        modelBuilder.Entity<ProfundumEinschreibung>(e =>
        {
            e.HasOne(e => e.ProfundumInstanz).WithMany(pi => pi.Einschreibungen);
            e.HasOne(e => e.BetroffenePerson).WithMany(pe => pe.ProfundaEinschreibungen);
            e.HasKey(b => new { b.BetroffenePersonId, b.ProfundumInstanzId, });
        });
        modelBuilder.Entity<ProfundumBelegWunsch>(w =>
        {
            w.HasKey(b => new { b.ProfundumInstanzId, b.BetroffenePersonId, b.Stufe });
            w.HasOne(b => b.BetroffenePerson).WithMany(p => p.ProfundaBelegwuensche);
        });

        modelBuilder.Entity<CalendarSubscription>(s =>
        {
            s.HasOne(b => b.BetroffenePerson).WithMany();
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

        modelBuilder.Entity<OtiumWiederholung>()
            .Property(w => w.Block)
            .HasDefaultValueSql("''");
    }
}
