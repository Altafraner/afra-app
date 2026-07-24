namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     The full set of information a student needs to submit a ranked Belegwunsch: the eligible Profunda they may
///     rank, any slots already fixed/finalized, and the submission thresholds to validate against client-side.
/// </summary>
public record DTOProfundumKatalog
{
    /// <summary>
    ///     The Profunda the student is eligible to rank, given their grade level and Profil status.
    /// </summary>
    public required DTOKatalogEintrag[] Optionen { get; set; }

    /// <summary>
    ///     Slots the student is already fixed/finalized into and can no longer rank for.
    /// </summary>
    public required DTOFixierterSlot[] Fixiert { get; set; }

    /// <summary>
    ///     The canonical ids (see <see cref="Models.ProfundumSlot.ToString" />) of the currently open slots.
    /// </summary>
    public required string[] OffeneSlotIds { get; set; }

    /// <summary>
    ///     The minimum number of Profunda that must be ranked for a submission to be valid.
    /// </summary>
    public required int MinBelegWuensche { get; set; }

    /// <summary>
    ///     The minimum number of ranked Profunda that must offer an Instanz in each open slot.
    /// </summary>
    public required int MinWuenschePerSlot { get; set; }

    /// <summary>
    ///     The student's currently saved ranked Profundum-Definition ids, in preference order (index 0 = rank 1) -
    ///     either a draft in progress or a final submission, see <see cref="IstAbgegeben" />. Empty if nothing has
    ///     been saved yet for the open Einwahlzeitraum.
    /// </summary>
    public required Guid[] AktuelleWuensche { get; set; }

    /// <summary>
    ///     Whether <see cref="AktuelleWuensche" /> is a final submission (<c>true</c>) or an unfinished draft the
    ///     student saved without submitting (<c>false</c>).
    /// </summary>
    public required bool IstAbgegeben { get; set; }
}
