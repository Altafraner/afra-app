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
    ///     The ranked Profundum-Definition ids the Einwahl form should show/restore, in preference order (index 0 =
    ///     rank 1) - the student's in-progress draft, since that's what they're actively editing, falling back to
    ///     their final submission (<see cref="AbgegebeneWuensche" />) only if no draft has been saved yet. Empty if
    ///     nothing has been saved at all for the open Einwahlzeitraum.
    /// </summary>
    public required Guid[] AktuelleWuensche { get; set; }

    /// <summary>
    ///     The student's actual final submission, in preference order - independent of <see cref="AktuelleWuensche" />,
    ///     which may have since diverged from this if the student kept editing their draft afterward. Empty if they
    ///     haven't submitted for the open Einwahlzeitraum yet (see <see cref="IstAbgegeben" />).
    /// </summary>
    public required Guid[] AbgegebeneWuensche { get; set; }

    /// <summary>
    ///     Whether the student has a final submission on file for the open Einwahlzeitraum at all - independent of
    ///     any draft edits made since; the submission itself is never silently overwritten by further draft saves.
    /// </summary>
    public required bool IstAbgegeben { get; set; }
}
