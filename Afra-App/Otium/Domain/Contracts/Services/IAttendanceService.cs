using System.Security.Claims;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Contracts.Services;

/// <summary>
///     A service interface for managing attendance in the Otium module
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    ///     Gets the attendance status for a specific enrollment
    /// </summary>
    /// <param name="enrollmentId">The id of the enrollment to get the attendance for.</param>
    /// <returns>The <see cref="OtiumAnwesenheitsStatus" /> for the enrollment.</returns>
    /// <exception cref="KeyNotFoundException">The enrollmentId could not be found.</exception>
    Task<OtiumAnwesenheitsStatus> GetAttendanceForEnrollmentAsync(Guid enrollmentId);

    /// <summary>
    ///     Gets the attendance status for a person in a specific block
    /// </summary>
    /// <param name="blockId">The id of the block the attendance is for.</param>
    /// <param name="personId">The id of the person the attendace is for.</param>
    /// <returns>The <see cref="OtiumAnwesenheitsStatus" /> for the enrollment.</returns>
    Task<OtiumAnwesenheitsStatus> GetAttendanceForStudentInBlockAsync(Guid blockId, Guid personId);

    /// <summary>
    ///     Gets the attendance status for all enrollments in a specific termin
    /// </summary>
    /// <param name="terminId">The termin to get all enrollments for</param>
    /// <returns>
    ///     A dictionary containing the <see cref="OtiumEinschreibung">Einschreibungen</see> along with their
    ///     <see cref="OtiumAnwesenheitsStatus" />.
    /// </returns>
    Task<Dictionary<Person, OtiumAnwesenheitsStatus>> GetAttendanceForTerminAsync(Guid terminId);

    /// <summary>
    ///     Gets the attendance status for all enrollments in a specific block
    /// </summary>
    /// <param name="blockId">The block to get all enrollments for</param>
    /// <returns>
    ///     A dictionary containing all enrolled students attendance status grouped by their termin along with all
    ///     the attendance statuses for students not enrolled
    /// </returns>
    Task<(Dictionary<OtiumTermin, Dictionary<Person, OtiumAnwesenheitsStatus>> termine,
            Dictionary<Person, OtiumAnwesenheitsStatus> missingPersons, bool missingPersonsChecked)>
        GetAttendanceForBlockAsync(Guid blockId);

    /// <summary>
    ///     Retrieves the attendance status for a list of blocks for a specific student.
    /// </summary>
    /// <param name="blockIds">The blocks to retrieve the attendances for</param>
    /// <param name="personId">The student whose attendances should be retrieved</param>
    /// <returns>A dictionary with the key being the blocks ID and the value the attendance status for the block with the id</returns>
    Task<Dictionary<Guid, OtiumAnwesenheitsStatus>> GetAttendanceForBlocksAsync(IEnumerable<Guid> blockIds,
        Guid personId);

    /// <summary>
    ///     Sets the attendance status for a specific enrollment
    /// </summary>
    /// <param name="enrollmentId">The enrollment to set the status for</param>
    /// <param name="status">The status to set</param>
    /// <exception cref="KeyNotFoundException">The enrollment does not exist</exception>
    Task SetAttendanceForEnrollmentAsync(Guid enrollmentId, OtiumAnwesenheitsStatus status);

    /// <summary>
    ///     Sets the attendance status for a specific student in a specific block
    /// </summary>
    /// <param name="studentId">The <see cref="Guid" /> of the <see cref="Person">Student</see>.</param>
    /// <param name="blockId">The <see cref="Guid" /> of the <see cref="Block" /></param>
    /// <param name="status">The status to set</param>
    /// <exception cref="KeyNotFoundException">The student or block does not exist.</exception>
    Task SetAttendanceForStudentInBlockAsync(Guid studentId, Guid blockId, OtiumAnwesenheitsStatus status);

    /// <summary>
    ///     Sets the checked status for a specific termin
    /// </summary>
    /// <param name="terminId">The id of the termin</param>
    /// <param name="status">The new status</param>
    Task SetStatusForTerminAsync(Guid terminId, bool status);

    /// <summary>
    ///     Sets the checked status for checking on missing persons in a specific block
    /// </summary>
    /// <param name="blockId">The Id of the block</param>
    /// <param name="status">The status to set</param>
    Task SetStatusForMissingPersonsAsync(Guid blockId, bool status);

    /// <summary>
    ///     Checks if a user may supervise the attendance for a given block.
    /// </summary>
    bool MaySupervise(ClaimsPrincipal user, Block block);
}
