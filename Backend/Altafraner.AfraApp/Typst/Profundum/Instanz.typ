#import "@preview/cmarker:0.1.8"
#import "@preview/mitex:0.2.6": mitex

#let (
    bezeichnung,
    slots,
    beschreibung,
    ort,
    voraussetzungen,
    verantwortliche,
    teilnehmer
) = json(bytes(sys.inputs.data))

#let text-color = rgb("#000")
#let gray-color = rgb("#aaa")
#let accent-color = rgb("#0069B4")
#let accent-font = ("TheSerif", "TimesNewRoman", "Noto Serif", "Noto Sans", "Arial")
#let primary-font = ("TheSansOSF", "Noto Sans", "Arial")

#let weekdays = (
  "Sonntag",
  "Montag",
  "Dienstag",
  "Mittwoch",
  "Donnerstag",
  "Freitag",
  "Samstag",
)

#show heading.where(level: 1): set text(size: 22pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 2): set text(size: 17pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 3): set text(size: 14pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 4): set text(size: 12pt, font: accent-font, weight: 500, fill: accent-color)

#set page(
  paper: "a4",
  margin: (
    left: 15mm,
    top: 10mm,
    right: 10mm,
    bottom: 10mm
  ),
  footer: align(left)[
    #text(size: 7pt, fill: gray-color)[
      Generiert in der Afra-App am #datetime.today().display("[day padding:zero].[month repr:numerical padding:zero].[year repr:full]")
    ]
  ]
)
#set text(size: 12pt, fill: text-color, font: primary-font)

#grid(
    columns: (1fr, 1fr),
    rows: (auto),
    align(left)[
        #v(1em)

        = #bezeichnung

        #grid(
            columns: (1fr, 1fr),
            rows: 3em,
            ..for s in slots {
                  ([*#weekdays.at(s.Wochentag) Q#s.Quartal*\
                  Schuljahr #s.Jahr/#{calc.rem-euclid(s.Jahr + 1, 100)}],)
              }
        )
    ],
    align(right, image("logo.png", width: 50mm))
)

Raum #ort

== Beschreibung

#cmarker.render(math:mitex,h1-level:3,
    beschreibung
)

#if (voraussetzungen.len() > 0) {[
    === Vorausgesetzte Profunda
    #for v in voraussetzungen {
      [- #v ]
    }
]}

#if (verantwortliche.len() > 0) {[
    == Verantwortliche

    #table(columns: 3,stroke:none,
        ..for v in verantwortliche {
            (v.Nachname, v.Vorname, v.Email)
        }
    )
]}

== Teilnehmer:innen

#for v in teilnehmer {
  [- #v.Nachname, #v.Vorname (#v.Gruppe)]
}
