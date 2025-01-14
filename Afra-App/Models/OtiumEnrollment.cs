using System.ComponentModel.DataAnnotations;

namespace Afra_App.Models;

public class OtiumEnrollment
{
    [Key]
    public Guid Id { get; set; }
    public required OstiumInstallment Installment { get; set; }
    public required Person Student { get; set; }
    public required TimeOnly Start { get; set; }
    public required TimeOnly End { get; set; }
    public bool AttendanceVerified { get; set; } = false;
    public bool MayEdit { get; set; } = false;
    
    public void VerifyAttendance() => AttendanceVerified = true;
}