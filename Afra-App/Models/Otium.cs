using System.ComponentModel.DataAnnotations;

namespace Afra_App.Models;

public class Otium
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public required string Designation { get; set; }
    [MaxLength(500)]
    public required string Description { get; set; }
    public required bool IsCataloged { get; set; }

    public required OtiumsKategory Kategory { get; set; }
    public ICollection<Person> Managers { get; set; } = new List<Person>();

    public ICollection<OtiumRegularity> Regularities { get; set; } = new List<OtiumRegularity>();

    public ICollection<OstiumInstallment> Installments { get; set; } = new List<OstiumInstallment>();

    public void EnrollOnce(Person person, DateTime start, TimeOnly end, bool mayEdit)
    {
        // Try find a event that matches the given time
        var termin = FindOrCreateEvent(start, end);

        if (termin is null)
            throw new InvalidOperationException("No event found or created for the given time.");

        // Enroll the person
        termin.Enroll(person, TimeOnly.FromDateTime(start), end, mayEdit);
    }

    /// <summary>
    /// Finds an instance of <see cref="OstiumInstallment"/> that matches the given time. If no such instance exists but a recurring event exists, a new instance is created. If no such instance exists, <see langword="null"/> is returned.
    /// </summary>
    /// <param name="start">DateTime that falls inside the window of the installment</param>
    /// <param name="ende">TimeOnly that falls inside the installments window</param>
    /// <returns><c>null</c> if there is no installment, otherwise the installment that contains the given installment</returns>
    private OstiumInstallment? FindOrCreateEvent(DateTime start, TimeOnly ende)
    {
        // Try find existing installment
        var installments = Installments
            .Where(t => t.Start <= start && t.End >= ende);

        var installment = installments.FirstOrDefault(t => !t.IsCanceled) ??
                          installments.FirstOrDefault(t => t.Regularity is not null && t.IsCanceled);

        // BUG: If there are multiple regularities that match the given time, the first one with a canceled installment will be found and null will be returned if the other regularities have no installment that is not cancelled.
        switch (installment)
        {
            case { IsCanceled: false }:
                return installment;
            case { IsCanceled: true }:
                return null;
        }

        // Try find regularity
        // BUG: This will only find the first regularity that matches the given time, there could be multiple.
        var regularity = Regularities
            .FirstOrDefault(r =>
                r.Day == start.DayOfWeek &&
                r.Start <= TimeOnly.FromDateTime(start) &&
                r.End >= ende);

        if (regularity is null) return null;

        // Create new installment from regularity
        installment = new OstiumInstallment
        {
            Otium = this,
            Tutor = regularity.Tutor,
            Start = start.Date + regularity.Start.ToTimeSpan(),
            End = regularity.End,
            Location = regularity.Location,
            Regularity = regularity
        };

        Installments.Add(installment);
        return installment;
    }
}