const student1 = {
  vorname: "Maggie",
  nachname: "Simpson"
}

const student2 = {
  vorname: "Lisa",
  nachname: "Simpson"
}

const student3 = {
  vorname: "Barth",
  nachname: "Simpson"
}

const teacher1 = {
  vorname: "Marge",
  nachname: "Simpson"
}

const teacher2 = {
  vorname: "Homer",
  nachname: "Simpson"
}

const studienzeitBase = {
  bezeichnung: "Studienzeit Mathematik",
  kategorien: ["0", "0-1"],
  beschreibung: "Dieses Angebot erlaubt den Schüler:innen und Schülern Hilfe zu mathematischen Aufgabenstellungen zu erhalten. \n\n Es ist ein freiwilliges Angebot, dass alle Schüler:innen und Schüler wahrnehmen können.",
  tutor: teacher1,
  ort: "110",
}

export const kategorien = [
  {
    bezeichnung: "Akademisches",
    id: "0",
    icon: "pi pi-graduation-cap",
    cssColor: "var(--p-blue-500)",
    children: [
      {
        bezeichnung: "Studienzeit",
        id: "0-1"
      },
      {
        bezeichnung: "Schüler unterrichten Schüler",
        id: "0-2"
      },
      {
        bezeichnung: "Wettbewerbe",
        id: "0-3"
      },
      {
        bezeichnung: "Sonstiges",
        id: "0-4"
      }
    ]
  },
  {
    bezeichnung: "Bewegung",
    id: "3",
    icon: "pi pi-heart",
    cssColor: "var(--p-teal-500)"
  },
  {
    bezeichnung: "Musik",
    id: "4",
    icon: "pi pi-headphones",
    cssColor: "var(--p-orange-500)"
  },
  {
    bezeichnung: "Besinnung",
    id: "5",
    icon: "pi pi-hourglass",
    cssColor: "var(--p-yellow-500)"
  },
  {
    bezeichnung: "Beratung",
    id: "6",
    icon: "pi pi-user",
    cssColor: "var(--p-purple-500)"
  },
  {
    bezeichnung: "Teamräume",
    id: "7"
  }
]

export const otium = Object.assign({
  termine: [
    {
      datum: new Date(),
      block: 1,
      tutor: teacher1,
    },
    {
      datum: new Date(),
      block: 2,
      tutor: teacher2,
    }],
  wiederholungen: [
    {
      wochentyp: "N",
      wochentag: "Montag",
      block: 1,
      tutor: teacher1
    },
    {
      wochentyp: "N",
      wochentag: "Freitag",
      block: 2,
      tutor: {
        vorname: "Marge",
        nachname: "Simpson"
      }
    }
  ],
  verwaltende: [
    teacher1,
    teacher2
  ]
}, studienzeitBase)

const terminEinschreibungsDetailsBase = {
  block: 1,
  datum: new Date(),
  tutor: teacher1,
  kontrolliert: false,
  einschreibungen: [
    {
      interval: {
        start: "13:30",
        end: "14:15",
      },
      student: student1,
      verified: 1
    },
    {
      interval: {
        start: "13:45",
        end: "14:30",
      },
      student: student2,
      verified: 0
    },
    {
      interval: {
        start: "13:30",
        end: "14:45",
      },
      student: student3,
      verified: 2
    }
  ]
}

export const terminEinschreibungsDetails = Object.assign({
  id: "1",
}, terminEinschreibungsDetailsBase, studienzeitBase)

export const supervisionDetails = [
  Object.assign({
    id: "0",
    bezeichnung: "Nicht eingeschrieben",
    ort: "FEHLEND"
  }, terminEinschreibungsDetailsBase),
  terminEinschreibungsDetails,
  Object.assign({
    id: "2",
    bezeichnung: "Schüler unterrichten Schüler",
    ort: "109"
  }, terminEinschreibungsDetailsBase),
  Object.assign({
    id: "3",
    bezeichnung: "Studienberatung",
    ort: "108"
  }, terminEinschreibungsDetailsBase),
  Object.assign({
    id: "4",
    bezeichnung: "Ruheraum",
    ort: "106"
  }, terminEinschreibungsDetailsBase),
  Object.assign({
    id: "5",
    bezeichnung: "Nachschreiben",
    ort: "105"
  }, terminEinschreibungsDetailsBase),
  Object.assign({
    id: "6",
    bezeichnung: "Yoga",
    ort: "104"
  }, terminEinschreibungsDetailsBase),
  Object.assign({
    id: "103",
    bezeichnung: "Schreibwerkstatt",
    ort: "102"
  }, terminEinschreibungsDetailsBase)
]
