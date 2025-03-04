using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;
using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.Otium;

public class Einschreibung
{
    [Key]
    public Guid Id { get; set; }
    public required Termin Termin { get; set; }
    public required Person BetroffenePerson { get; set; }
    public required TimeOnlyInterval Interval { get; set; }
}