#let data = json("inputs.json")

#let text-color = rgb("#000")
#let gray-color = rgb("#aaa")
#let accent-color = rgb("#0069B4")
#let accent-font = ("TheSerif", "TimesNewRoman", "Noto Serif", "Noto Sans", "Arial")
#let primary-font = ("TheSansOSF", "Noto Sans", "Arial")

#let status-labels = (
  Gestellt: "Eingereicht",
  AlleLehrerGenehmigt: "Alle Lehrkräfte haben genehmigt",
  Abgelehnt: "Abgelehnt",
  Bestaetigt: "Vom Sekretariat bestätigt",
  SchulleiterBestaetigt: "Genehmigt",
  SekretariatAbgelehnt: "Vom Sekretariat abgelehnt",
  SchulleiterAbgelehnt: "Vom Schulleiter abgelehnt",
)

#let entscheidung-labels = (
  Ausstehend: "Ausstehend",
  Genehmigt: "Genehmigt",
  Abgelehnt: "Abgelehnt",
)

#let format-date(d) = {
  let parts = str(d).split("-")
  if parts.len() == 3 {
    parts.at(2) + "." + parts.at(1) + "." + parts.at(0)
  } else {
    str(d)
  }
}

#let format-datetime(dt) = {
  let s = str(dt)
  // Handle ISO date-time strings like "2025-03-01T08:00:00Z"
  let date-part = s.split("T").at(0, default: s)
  format-date(date-part)
}

#show heading.where(level: 1): set text(size: 22pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 2): set text(size: 14pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 3): set text(size: 12pt, font: accent-font, weight: 500, fill: accent-color)

#set page(
  paper: "a4",
  margin: (
    left: 20mm,
    top: 15mm,
    right: 20mm,
    bottom: 20mm
  ),
  footer: align(left)[
    #text(size: 7pt, fill: gray-color)[
      Generiert in der Afra-App am #datetime.today().display("[day padding:zero].[month repr:numerical padding:zero].[year repr:full]")
    ]
  ]
)
#set text(size: 11pt, fill: text-color, font: primary-font)

// Header with logo
#grid(
    columns: (1fr, auto),
    rows: (auto),
    column-gutter: 2em,
    align(left)[
        #v(0.5em)
        = Freistellungsantrag
    ],
    align(right, image("logo.png", width: 50mm))
)

#v(0.5em)

// Student info and date range
#grid(
    columns: (1fr, 1fr),
    row-gutter: 0.6em,
    [*Schüler:in:* #data.Student.Vorname #data.Student.Nachname #if data.Student.Gruppe != none [(#data.Student.Gruppe)]],
    align(right)[*Status:* #status-labels.at(data.Status, default: data.Status)],
    [*Zeitraum:* #format-datetime(data.Von) – #format-datetime(data.Bis)],
    align(right)[*Erstellt am:* #format-datetime(data.ErstelltAm)],
)

#v(0.8em)

// Reason
== Grund
*#data.Grund*

#if data.Beschreibung.len() > 0 [
  #v(0.3em)
  #data.Beschreibung
]

#v(0.8em)

// Missed lessons table
#if data.BetroffeneStunden.len() > 0 [
  == Betroffene Stunden

  #table(
    columns: (auto, auto, 1fr, 1fr),
    inset: 6pt,
    stroke: 0.5pt + gray-color,
    align: (left, center, left, left),
    table.header(
      [*Datum*], [*Block*], [*Fach*], [*Lehrkraft*],
    ),
    ..for s in data.BetroffeneStunden {
      (
        format-date(s.Datum),
        str(s.Block),
        s.Fach,
        [#s.Lehrer.Vorname #s.Lehrer.Nachname],
      )
    }
  )

  #v(0.5em)
]

// Teacher/Mentor decisions
#if data.Entscheidungen.len() > 0 [
  == Entscheidungen der Lehrkräfte und Mentor:innen

  #table(
    columns: (1fr, auto, 1fr),
    inset: 6pt,
    stroke: 0.5pt + gray-color,
    align: (left, center, left),
    table.header(
      [*Lehrkraft / Mentor:in*], [*Entscheidung*], [*Kommentar*],
    ),
    ..for e in data.Entscheidungen {
      (
        [#e.Lehrer.Vorname #e.Lehrer.Nachname],
        entscheidung-labels.at(e.Status, default: e.Status),
        if e.Kommentar != none { e.Kommentar } else { [–] },
      )
    }
  )

  #v(0.5em)
]

// Sekretariat / Schulleiter comments if present
#if data.SekretariatKommentar != none [
  === Kommentar Sekretariat
  #data.SekretariatKommentar
  #v(0.3em)
]

#if data.SchulleiterKommentar != none [
  === Kommentar Schulleiter
  #data.SchulleiterKommentar
  #v(0.3em)
]

// Signature lines
#v(2em)

#grid(
    columns: (1fr, 1fr),
    column-gutter: 3em,
    [
        #line(length: 100%, stroke: 0.5pt + gray-color)
        #v(0.2em)
        #text(size: 9pt, fill: gray-color)[Datum, Unterschrift Sekretariat]
    ],
    [
        #line(length: 100%, stroke: 0.5pt + gray-color)
        #v(0.2em)
        #text(size: 9pt, fill: gray-color)[Datum, Unterschrift Schulleiter:in]
    ],
)
