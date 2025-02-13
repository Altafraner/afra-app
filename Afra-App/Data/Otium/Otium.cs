using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Otium;

public class Otium
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public required string Bezeichnung { get; set; }
    [MaxLength(500)]
    public required string Beschreibung { get; set; }
    public required Kategorie Kategorie { get; set; }
    public ICollection<Person> Verantwortliche { get; set; } = new List<Person>();

    public ICollection<Wiederholung> Wiederholungen { get; set; } = new List<Wiederholung>();

    public ICollection<Termin> Termine { get; set; } = new List<Termin>();

    public void EinmalEinschreiben(Person person, DateTime start, TimeOnly ende)
    {
        // Try find a event that matches the given time
        var termin = FindeOderErstelleTermin(start, ende);

        if (termin is null)
            throw new InvalidOperationException("No event found or created for the given time.");

        // Enroll the person
        termin.Einschreiben(person, TimeOnly.FromDateTime(start), ende);
    }

    /// <summary>
    /// Finds an instance of <see cref="Termin"/> that matches the given time. If no such instance exists but a recurring event exists, a new instance is created. If no such instance exists, <see langword="null"/> is returned.
    /// </summary>
    /// <param name="start">DateTime that falls inside the window of the installment</param>
    /// <param name="ende">TimeOnly that falls inside the installments window</param>
    /// <returns><c>null</c> if there is no installment, otherwise the installment that contains the given installment</returns>
    private Termin? FindeOderErstelleTermin(DateTime start, TimeOnly ende)
    {
        // Try find existing installment
        var termine = Termine
            .Where(t => t.Datum == DateOnly.FromDateTime(start) && t.Block == GetBlockFromTime(start))
            .ToList();

        var termin = termine.FirstOrDefault(t => !t.IstAbgesagt) ??
                          termine.FirstOrDefault(t => t.Wiederholung is not null && t.IstAbgesagt);

        // BUG: If there are multiple regularities that match the given time, the first one with a canceled installment will be found and null will be returned if the other regularities have no installment that is not cancelled.
        switch (termin)
        {
            case { IstAbgesagt: false }:
                return termin;
            case { IstAbgesagt: true }:
                return null;
        }

        // Try find regularity
        // BUG: This will only find the first regularity that matches the given time, there could be multiple.
        var wiederholung = Wiederholungen
            .FirstOrDefault(r =>
                r.Wochentag == start.DayOfWeek &&
                r.Block == GetBlockFromTime(start));

        if (wiederholung is null) return null;

        // Create new installment from regularity
        termin = new Termin
        {
            Otium = this,
            Tutor = wiederholung.Tutor,
            Datum = DateOnly.FromDateTime(start.Date),
            Block = wiederholung.Block,
            Ort = wiederholung.Location,
            Wiederholung = wiederholung
        };

        Termine.Add(termin);
        return termin;
    }

    private byte GetBlockFromTime(DateTime start)
    {
        throw new NotImplementedException();
    }
}