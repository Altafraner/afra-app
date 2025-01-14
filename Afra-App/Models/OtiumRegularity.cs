using System.ComponentModel.DataAnnotations;

namespace Afra_App.Models;

public class OtiumRegularity
{
    public Guid Id { get; init; }
    public required Otium Otium { get; set; }
    public required Person Tutor { get; set; }
    public required DayOfWeek Day { get; set; }
    public required TimeOnly Start { get; set; }
    public required TimeOnly End { get; set; }
    
    [MaxLength(10)]
    public required string Location { get; set; }
    
    public ICollection<OstiumInstallment> Installments { get; init; } = new List<OstiumInstallment>();

    public OstiumInstallment CreateInstallment(DateOnly date)
    {
        if (date.DayOfWeek != Day)
            throw new ArgumentException("The given date does not match the regularity's day of the week.");

        var installment = new OstiumInstallment
        {
            Otium = Otium,
            Tutor = Tutor,
            Start = date.ToDateTime(Start),
            End = End,
            Location = Location,
            Regularity = this
        };
        
        Installments.Add(installment);
        return installment;
    }
}