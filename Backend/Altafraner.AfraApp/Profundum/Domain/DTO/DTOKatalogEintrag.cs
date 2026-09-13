namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A single Profundum (topic) a student may rank, as offered in a <see cref="DTOProfundumKatalog" />.
/// </summary>
public record DTOKatalogEintrag
{
    /// <summary>
    ///     The id of the <see cref="Models.ProfundumDefinition" />.
    /// </summary>
    public required Guid DefinitionId { get; set; }

    /// <summary>
    ///     The name of the Profundum.
    /// </summary>
    public required string Bezeichnung { get; set; }

    /// <summary>
    ///     Whether this Profundum counts as a Profilprofundum.
    /// </summary>
    public required bool ProfilProfundum { get; set; }

    /// <summary>
    ///     The canonical ids of the open slots this Profundum's eligible Instanzen occupy.
    /// </summary>
    public required string[] SlotIds { get; set; }

    /// <summary>
    ///     Whether the organizer allows a mutual team-partner wish for this Profundum.
    /// </summary>
    public required bool ErlaubtPartnerwahl { get; set; }

    /// <summary>
    ///     The Profundum's free-text description, for a student-facing detail view.
    /// </summary>
    public required string Beschreibung { get; set; }

    /// <summary>
    ///     The labels of the Fachbereiche (subject departments) this Profundum belongs to.
    /// </summary>
    public required string[] Fachbereiche { get; set; }

    /// <summary>
    ///     The names of prerequisite Profunda (<see cref="Models.ProfundumDefinition.Dependencies" />) a student
    ///     must already be enrolled in.
    /// </summary>
    public required string[] Voraussetzungen { get; set; }

    /// <summary>
    ///     The concrete offered Instanzen (sections) of this Profundum still open for this student, each with its
    ///     own location and supervisors.
    /// </summary>
    public required DTOKatalogEintragInstanz[] Instanzen { get; set; }
}

/// <summary>
///     A single concrete Instanz (offered section) of a Profundum, as shown in a student-facing detail view.
/// </summary>
public record DTOKatalogEintragInstanz
{
    /// <summary>
    ///     The canonical ids (see <see cref="Models.ProfundumSlot.ToString" />) of this Instanz's own open slots.
    /// </summary>
    public required string[] SlotIds { get; set; }

    /// <summary>
    ///     The physical location this Instanz takes place at.
    /// </summary>
    public required string Ort { get; set; }

    /// <summary>
    ///     The names of the staff members supervising this Instanz.
    /// </summary>
    public required string[] Verantwortliche { get; set; }

    /// <summary>
    ///     The maximum number of students that may enroll in this Instanz, if capped.
    /// </summary>
    public int? MaxEinschreibungen { get; set; }
}
