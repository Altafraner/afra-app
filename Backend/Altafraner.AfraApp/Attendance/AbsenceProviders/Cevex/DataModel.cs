using System.Diagnostics.CodeAnalysis;

namespace Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;

internal class CevexUser
{
    public CevexUser()
    {
    }

    [SetsRequiredMembers]
    internal CevexUser(CevexUserOriginal original)
    {
        Guid = original.guid;
        Classname = original.classname;
        Firstname = original.firstname;
        Lastname = original.lastname;
        Missings = original.missings.Select(e => new Missing(e)).ToArray();
    }

    public required string Guid { get; init; }
    public required string Classname { get; init; }
    public required string Firstname { get; init; }
    public required string Lastname { get; init; }
    public required Missing[] Missings { get; init; }
}

internal class CevexUserOriginal
{
    public required string guid { get; set; }
    public required string classname { get; set; }
    public required string firstname { get; set; }
    public required string lastname { get; set; }
    public MissingOriginal[] missings { get; set; } = [];
}

internal class Missing
{
    public Missing()
    {
    }

    [SetsRequiredMembers]
    internal Missing(MissingOriginal original)
    {
        Memo = original.memo;
        Date = DateOnly.Parse(original.date);
        Fullday = original.fullday == 1;
        Inlesson = original.inlesson;
        Lessons = original.lessons;
        Missingtype = (MissingType)original.missingtype;
    }

    public required string Memo { get; set; }
    public required DateOnly Date { get; set; }
    public required bool Fullday { get; set; }
    public required int Inlesson { get; set; }
    public required int Lessons { get; set; }
    public required MissingType Missingtype { get; set; }
}

internal class MissingOriginal
{
    public required string memo { get; set; }
    public required string date { get; set; }
    public required int fullday { get; set; }
    public required int inlesson { get; set; }
    public required int lessons { get; set; }
    public required int missingtype { get; set; }
}

internal enum MissingType
{
    Entschuldigt = 0,
    Unentschuldigt = 1,
    Unklar = 2,
    Schulevent = 3
}
