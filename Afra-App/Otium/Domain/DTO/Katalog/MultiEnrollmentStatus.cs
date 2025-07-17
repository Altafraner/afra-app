namespace Afra_App.Otium.Domain.DTO.Katalog;

public record MultiEnrollmentStatus(IEnumerable<DateOnly> approved, IEnumerable<DateOnly> denied);
