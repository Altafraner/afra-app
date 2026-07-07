#{
let (
    bezeichnung,
    slots,
    beschreibung,
    ort,
    voraussetzungen,
    verantwortliche,
    teilnehmer
) = json("inputs.json")

let font-size = 12pt
let primary-color = color.rgb("0069B4")
let weekdays = (
  "Sonntag",
  "Montag",
  "Dienstag",
  "Mittwoch",
  "Donnerstag",
  "Freitag",
  "Samstag",
)

let place_absolute(content, dx: 0pt, dy: 0pt) = {
  context place(left + top, dx: -page.margin.left + dx, dy: -page.margin.top + dy, content)
}

set page(
    margin: (
    left: 25mm,
    right: 20mm,
    top: 25mm,
    bottom: 15mm
  ),
  footer: text(
    weight: 250,
    size: 8pt,
    "Generiert in der Afra-App am " + datetime.today().display("[day].[month].[year]")
  )
)

set text(
  lang: "de",
  font: "TheSansOsF",
  size: font-size,
  weight: 400
)

show title: set text(font: "TheSerif", weight: 700, size: 2 * font-size)
show heading: set text(font: "TheSerif", weight: 400, size: font-size)
show heading.where(level: 1): set text(weight: 700, size: 1.7em)
show heading.where(level: 2): set text(weight: 700, size: 1.4em)

context place(
  top + right,
  dy: -page.margin.top + 10mm,
  float: true,
  image("logo.png", width: 28.6%)
)

title(bezeichnung)

grid(
  columns: (1fr, 1fr, 1fr, 1fr),
  gutter: 1em,
  ..slots.map(s => {
    text(weight: 530)[#weekdays.at(s.Wochentag) Q#s.Quartal]
    linebreak()
    [Schuljahr #s.Jahr/#{calc.rem-euclid(s.Jahr + 1, 100)}]
  }),
)

let quickfacts = (
  ..if ort.len() > 0 {([Raum],ort)},
  ..if verantwortliche.len() > 0 {([Betreuer:in],verantwortliche.map(v => [#v.Vorname #v.Nachname]).join(", "))},
  ..if voraussetzungen.len() > 0 {([Voraussetzungen], voraussetzungen.join(", "))}
)

v(2em)

if (quickfacts.len() > 0) {
  table(
    columns: (auto, auto),
    stroke: none,
    inset: 0pt,
    row-gutter: 0.85em,
    column-gutter: 1em,
    ..quickfacts
  )
  v(2em)
}

heading(depth: 2)[Teilnehmer:innen]

if (teilnehmer.len() == 0) [
  Keine Einschreibungen
] else {
  table(
    columns: (auto, auto),
    align: (left, right),
    stroke: none,
    inset: 0pt,
    row-gutter: 0.85em,
    column-gutter: 1em,
    table.header(text(weight: 520)[Name], text(weight: 520)[Klasse]),
    ..(teilnehmer.map(e => (e.Nachname + ", " + e.Vorname, text(weight: 250, e.Gruppe)))).flatten()
  )
}
}
