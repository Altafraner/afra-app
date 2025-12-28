#import "@preview/cmarker:0.1.8"
#import "@preview/mitex:0.2.6": mitex

#let bezeichnung = sys.inputs.bezeichnung
#let beschreibung = sys.inputs.beschreibung
#let slots = json(bytes(sys.inputs.slots))
#let verantwortliche = json(bytes(sys.inputs.verantwortliche))
#let teilnehmer = json(bytes(sys.inputs.teilnehmer))

#align(right, image("logo.png", width: 50mm))

#let accent-color = rgb("#0069B4")
#let accent-font = "TheSerif"

#show heading.where(level: 1): set text(size: 22pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 2): set text(size: 17pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 3): set text(size: 14pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 4): set text(size: 12pt, font: accent-font, weight: 500, fill: accent-color)

= Profundum: #bezeichnung

#let weekdays = (
  "Sonntag",
  "Montag",
  "Dienstag",
  "Mittwoch",
  "Donnerstag",
  "Freitag",
  "Samstag",
)

#table(columns: 3,stroke:none,
    ..for s in slots {
        ([#s.Jahr/#{calc.rem-euclid(s.Jahr + 1, 100)}], [Q#s.Quartal], weekdays.at(s.Wochentag))
    }
)

== Inhalt


#cmarker.render(math:mitex,h1-level:3,
    beschreibung
)

== Verantwortliche

#table(columns: 3,stroke:none,
    ..for v in verantwortliche {
        (v.Nachname, v.Vorname, v.Email)
    }
)

== Teilnehmer

#table(columns: 3,
    ..for v in teilnehmer {
        (v.Gruppe, v.Nachname, v.Vorname,)
    }
)
