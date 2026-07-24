using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Configuration;

///
public class ProfundumConfiguration
{
    /// <summary>
    ///  When disabled all entries are randomized before a matching to improve fairness in unlikely cases.
    ///  Enable for debugging only.
    /// </summary>
    public required bool DeterministicMatching { get; set; }

    /// <summary>
    ///     A dictionary containing a grade level as key and list of quartals as value that describes when which grade level
    ///     must enroll in a profilprofundum
    /// </summary>
    public required Dictionary<int, ProfundumQuartal[]> ProfilPflichtigkeit { get; set; }

    /// <summary>
    ///     The minimum number of Profunda a student must rank in a single Belegwunsch submission.
    /// </summary>
    public int MinBelegWuensche { get; set; } = 7;

    /// <summary>
    ///     The minimum number of ranked Profunda that must offer an Instanz in each currently open Slot.
    /// </summary>
    public int MinWuenschePerSlot { get; set; } = 3;

    /// <summary>
    ///     The reward given for a rank-1 wish being satisfied, before the quadratic cost is subtracted. See
    ///     <see cref="WunschKostenFaktor" />.
    /// </summary>
    public int WunschBasisWert { get; set; } = 200;

    /// <summary>
    ///     Scales the convex (quadratic) cost of a satisfied wish's rank: cost = <see cref="WunschKostenFaktor" /> *
    ///     rang^2, subtracted from <see cref="WunschBasisWert" />. A larger factor makes the objective punish spreading
    ///     bad outcomes across students more aggressively, satisfying strong preferences at the expense of weak ones.
    /// </summary>
    public int WunschKostenFaktor { get; set; } = 1;

    /// <summary>
    ///     The reward floor for any ranked wish, regardless of how low its rank is. Kept comfortably above the "not
    ///     enrolled" reward (1) so that honoring even a very low-ranked wish always beats leaving a student
    ///     unenrolled.
    /// </summary>
    public int WunschMindestWert { get; set; } = 5;

    /// <summary>
    ///     Path to a plain text file, one word per line, used to generate the human-shareable Partnerwahl invite
    ///     tokens (see <see cref="Services.ProfundumPartnerTokenService" />).
    /// </summary>
    public required string PartnerWahlWordlistPath { get; set; }

    /// <summary>
    ///     The number of words drawn from <see cref="PartnerWahlWordlistPath" /> and joined with hyphens to form a
    ///     Partnerwahl invite token, e.g. 3 -&gt; "apfel-baum-schnee".
    /// </summary>
    public required int PartnerwahlTokenWordCount { get; set; }
}
