#import "@preview/cetz:0.4.2"
#import "@preview/titleize:0.1.1": titlecase

#let input = json(bytes(sys.inputs.data))

#set text(lang: "de")

#let accent-color = rgb("#0069B4")
#let text-color = rgb("#333333")
#let gray-color = rgb("#888888")
#let primary-font = "TheSansOsF"
#let accent-font = "TheSerif"

#let size-small = 0.8em
#let size-normal = 11pt
#let size-large = 1.2em
#let size-extralarge = 1.6em
#let size-huge = 30pt

#let calc-means(data) = {
  let means = ()
  for (cat, items) in data {
    let sum = 0.0
    let count = 0
    for (_, notes) in items {
      for n in notes {
        sum += n
        count += 1
      }
    }
    means.push(sum / count)
  }
  return means
}

#let radar-chart(data) = {
  let categories = data.keys()
  let values = calc-means(data)
  layout(ly => {
    cetz.canvas(length: 1cm, {
      let d = cetz.draw
      let radius = 1.8
      let n = categories.len()
      let center = (0.0, 0.0)
      for i in range(n) {
        let angle = 90deg + i * (360deg / n)
        let x = radius * calc.cos(angle)
        let y = radius * calc.sin(angle)
        d.line(center, (x, y), stroke: 0.5pt + gray-color)
        let lx = (radius + 1.2) * calc.cos(angle)
        let ly = (radius + 0.6) * calc.sin(angle)
        d.content((lx, ly), text(size: size-small, weight: 500, fill: text-color, categories.at(i)))
      }
      for r in range(1, 5) {
        let rad = r / 4 * radius
        d.circle(center, radius: rad, stroke: (paint: gray-color, dash: "dotted"))
      }
      let points = ()
      for i in range(n) {
        let angle = 90deg + i * (360deg / n)
        let val-radius = (values.at(i) / 4) * radius
        points.push((val-radius * calc.cos(angle), val-radius * calc.sin(angle)))
      }
      if points.len() >= 2 {
        d.line(..points, close: true, fill: accent-color.transparentize(80%), stroke: 1.5pt + accent-color)
      }
    })
  })
}

#let name-of(person) = {
  titlecase([#person.Vorname #person.Nachname])
}

#let dot-plot(notes) = {
  let x(i) = { 0.05 + 0.9 * ((i - 1) / 3) }
  layout(ly => {
    cetz.canvas(length: ly.width, {
      let d = cetz.draw

      // Grundlinie
      d.line((0, 0.1), (1, 0.1), stroke: 1pt + gray-color.lighten(50%))

      for i in range(1, 5) {
        let currx = x(i)
        d.line((currx, 0.08), (currx, 0.12), stroke: 0.5pt + gray-color)
      }

      let counts = (0, 0, 0, 0, 0)
      for n in notes {
        if n >= 1 and n <= 4 {
          counts.at(n) = counts.at(n) + 1
        }
      }

      for i in range(1, 5) {
        let count = counts.at(i)
        if count > 0 {
          let r = 0.016 + (count - 1) * 0.007
          d.circle((x(i), 0.1), radius: r, fill: accent-color, stroke: none)
        }
      }
    })
  })
}

#let bigheading(content, subheading) = {
  text(
    size: size-small,
    grid(
      columns: (50% + 2em, 1fr, 1fr, 1fr, 1fr),
      align(left, text(size: size-normal)[
        #text(size-extralarge, font: accent-font, weight: 500, fill: accent-color, content) \
        #text(size: size-small, style: "italic", fill: gray-color, subheading)
      ]),
      [nicht ausgeprägt],
      align(center, [wenig ausgeprägt]),
      align(center, [deutlich ausgeprägt]),
      align(right, [hervorragend ausgeprägt]),
    ),
  )
  v(5pt)
}


#let profundumToText(p) = {
  p.Label
  if p.Verantwortliche.len() > 0 {
    [ (#p.Verantwortliche.map(name-of).join(", "))]
  }
}

#let category-block(title, items) = {
  block(
    breakable: false,
    {
      block(below: 4pt, text(size-large, weight: 600, font: accent-font, fill: accent-color, title))
      table(
        columns: (50%, 50%),
        stroke: none,
        inset: (y: 2pt, x: 0pt),
        column-gutter: 20pt,
        align: left + horizon,
        ..for (anker, notes) in items {
          (
            box(inset: (y: 2pt), text(size: size-normal, anker)),
            box(height: 1.2em, width: 90%, dot-plot(notes)),
          )
        }
      )
    },
  )
  v(4pt)
}

#let nice(index, content) = {
  counter(page).update(1)
  let end-label = label("end-" + str(index))
  set page(
    paper: "a4",
    margin: (
      top: 3 * 11.2mm,
      left: 11.2mm,
      right: 11.2mm,
      bottom: 15mm,
    ),
    footer: context align(
      right,
      text(size: size-small)[
        Seite #counter(page).display("1", both: false) von #counter(page).at(query(end-label).first().location()).first()
      ],
    ),
    header: {
      place(right + top, dy: 11.2mm, image("SMK_009_P_4C_FLUSH.svg", width: 76.8mm))
      place(left + top, dy: 11.2mm, image("SANKT AFRA_SCHLEIFENQUADRAT_BREIT_BLAU_KORREKTUR.svg", height: 11.5mm))
    },
  )
  content
  [#metadata(none) #end-label]
}

#let render(element) = [
  #let schueler = element.Person
  #let meta = element.Meta
  #let profunda = element.Profunda
  #let daten_allgemein = element.FeedbackAllgemein
  #let daten_fachlich = element.FeedbackFachlich
  #let gm = element.GM
  #let schulleiter = element.Schulleiter

  #set text(size: size-normal, fill: text-color, font: primary-font)
  #columns(2)[
    #align(left)[
      #text(29pt, font: primary-font, weight: 500, fill: accent-color)[PROFUNDUM]\
      #text(size-large, weight: "light")[Feedback-Bogen]
    ]
    #v(1em)
    #text(size-large, font: accent-font, weight: 700, name-of(schueler))\
    Klasse #schueler.Gruppe

    Schuljahr #meta.Schuljahr / #(meta.Schuljahr + 1) \
    #text(weight: "thin")[
      #if meta.Halbjahr [Erstes Halbjahr\ ]
      Datum: #meta.Datum
    ]

    #colbreak()
    #align(center + horizon)[
      #radar-chart(daten_allgemein)
    ]
  ]

  #v(0.5cm)
  #bigheading("Allgemeine Kompetenzen", "Allgemeine Bewertungen aus allen Profunda")
  #for (cat, items) in daten_allgemein {
    category-block(cat, items)
  }

  #block(breakable: false, inset: 0%, outset: 0%, spacing: 0%)[
    #if daten_fachlich != none and daten_fachlich.len() > 0 {
      v(1.7em)
      bigheading("Fachspezifische Kompetenzen", "Spezifische Bewertungen der Fachbereiche")

      for (cat, items) in daten_fachlich {
        category-block(cat, items)
      }
    }
    #block(
      breakable: false,
      fill: accent-color.lighten(95%),
      inset: 1em,
      radius: 4pt,
      width: 100%,
      [
        #text(weight: 700, size: size-small, font: accent-font)[Grundlage dieses Feedbacks sind folgende Profunda:]\
        #text(size: size-small, profunda.map(p => profundumToText(p)).join(", "))
      ],
    )

    #align(center)[
      #block(breakable: false, width: 80%)[
        #v(5em)
        #grid(
          columns: (1fr, 1fr),
          gutter: 3em,
          [
            #line(length: 100%)
            #v(-8pt)
            #text(size: size-small)[ #if gm != none [#name-of(gm)]\ Gymnasiale(r) Mentor(in)]
          ],
          [
            #line(length: 100%)
            #v(-8pt)
            #text(size: size-small)[#if schulleiter != none [#name-of(schulleiter)]\ Schulleiter(in)]
          ],
        )
      ]
    ]
  ]
]

#let i = 0
#for element in input {
  (i = i + 1)

  nice(i, render(element))

  if (i != input.len()) {
    pagebreak(to: "odd")
  }
}
