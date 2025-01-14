using System.ComponentModel.DataAnnotations;

namespace Afra_App.Models;

public class OstiumInstallment
{
    public Guid Id { get; set; }
    public required Otium Otium { get; set; }
    public required Person Tutor { get; set; }
    public required DateTime Start { get; set; }
    public required TimeOnly End { get; set; }
    
    [MaxLength(10)]
    public required string Location { get; set; }
    public OtiumRegularity? Regularity { get; set; }
    
    public ICollection<OtiumEnrollment> Enrollments { get; set; } = new List<OtiumEnrollment>();
    public bool IsCanceled { get; set; }

    public OtiumEnrollment Enroll(Person student, TimeOnly start, TimeOnly end, bool mayEdit)
    {
        if (IsCanceled)
            throw new InvalidOperationException("The installment has been canceled.");
        
        var enrollment = new OtiumEnrollment
        {
            Installment = this,
            Student = student,
            Start = start,
            End = end,
            MayEdit = mayEdit
        };
        
        Enrollments.Add(enrollment);
        return enrollment;
    }
}