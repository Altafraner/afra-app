using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;

internal sealed class CevexAttendanceEntryProvider : IAttendanceAutomaticEntryProvider
{
    private readonly CevexDataParser _parser;

    public CevexAttendanceEntryProvider(CevexDataParser parser)
    {
        _parser = parser;
    }

    public async Task<Dictionary<Guid, AttendanceState>> GetEntriesPerStudent(
        AttendanceScope scope,
        Guid slotId,
        AttendanceSlotMetadata metadata)
    {
        var allMissings = await _parser.GetMatches();
        var date = metadata.StartDate;
        var lesson = metadata.StartLesson;

        var results = new Dictionary<Guid, AttendanceState>();

        foreach (var (studentId, personsMissings) in allMissings)
        {
            var applicableMissing = personsMissings.FirstOrDefault(IsApplicable);
            if (applicableMissing == null) continue;
            results.Add(studentId, AttendanceState.Entschuldigt);
        }

        return results;

        bool IsApplicable(Missing missing)
        {
            if (missing.Date != date || missing.Missingtype == MissingType.Unentschuldigt) return false;
            if (missing.Fullday ||
                (missing.Inlesson <= lesson && missing.Inlesson + missing.Lessons > lesson)) return true;
            return false;
        }
    }
}
