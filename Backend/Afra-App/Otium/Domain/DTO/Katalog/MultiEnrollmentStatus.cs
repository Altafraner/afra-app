namespace Altafraner.AfraApp.Otium.Domain.DTO.Katalog;

/// <summary>
/// Represents the status of a multi-enrollment request.
/// </summary>
/// <param name="Approved">A list of all approved enrollments</param>
/// <param name="Denied">A list of all denied enrollments</param>
public record MultiEnrollmentStatus(IEnumerable<DateOnly> Approved, IEnumerable<DateOnly> Denied);
