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

export const kategorien = [
  {
    name: "Akademisches",
    id: "0",
    icon: "pi pi-graduation-cap",
    color: "var(--p-blue-500)",
    children: [
      {
        name: "Studienzeit",
        id: "0-1"
      },
      {
        name: "Schüler unterrichten Schüler",
        id: "0-2"
      },
      {
        name: "Wettbewerbe",
        id: "0-3"
      },
      {
        name: "Sonstiges",
        id: "0-4"
      }
    ]
  },
  {
    name: "Bewegung",
    id: "3",
    icon: "pi pi-heart",
    color: "var(--p-teal-500)"
  },
  {
    name: "Musik",
    id: "4",
    icon: "pi pi-headphones",
    color: "var(--p-orange-500)"
  },
  {
    name: "Besinnung",
    id: "5",
    icon: "pi pi-hourglass",
    color: "var(--p-yellow-500)"
  },
  {
    name: "Beratung",
    id: "6",
    icon: "pi pi-user",
    color: "var(--p-purple-500)"
  },
  {
    name: "Teamräume",
    id: "7"
  }
]

export const otium = {
  bezeichnung: "Studienzeit Mathematik",
  kategorien: ["0", "0-1"],
  beschreibung: "Dieses Angebot erlaubt den Schüler:innen und Schülern Hilfe zu mathematischen Aufgabenstellungen zu erhalten. \n\n Es ist ein freiwilliges Angebot, dass alle Schüler:innen und Schüler wahrnehmen können.",
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
}

export const termin = {
  bezeichnung: "Studienzeit Mathematik",
  beschreibung: "Dieses Angebot erlaubt den Schüler:innen und Schülern Hilfe zu mathematischen Aufgabenstellungen zu erhalten. \n\n Es ist ein freiwilliges Angebot, dass alle Schüler:innen und Schüler wahrnehmen können.",
  auslastung: 0.5,
  maxEinschreibungen: 7,
  einschreibungen: [
    {
      anzahl: 5,
      eingeschrieben: false,
      kannBearbeiten: true
    },
    {
      anzahl: 4,
      eingeschrieben: true,
      kannBearbeiten: true
    },
    {
      anzahl: 5,
      eingeschrieben: true,
      kannBearbeiten: true
    },
    {
      anzahl: 3,
      eingeschrieben: false,
      kannBearbeiten: true
    },
    {
      anzahl: 7,
      eingeschrieben: false,
      kannBearbeiten: false
    }],
  tutor: teacher1,
  ort: "110",
  kategorien: ["0", "0-1"],
  id: "1"
}

export const blockzeiten = [
  '13:30',
  '13:45',
  '14:00',
  '14:15',
  '14:30',
  '14:45'
]

export const terminEinschreibungsDetails = {
  bezeichnung: "Studienzeit Mathematik",
  block: 1,
  datum: new Date(),
  tutor: teacher1,
  einschreibungen: [
    {
      start: new Date(0, 0, 0, 12, 0),
      ende: new Date(0, 0, 0, 12, 20),
      student: student3,
      verified: 1
    },
    {
      start: new Date(0, 0, 0, 12, 0),
      ende: new Date(0, 0, 0, 12, 20),
      student: student1,
      verified: 0
    }
  ]
}

export const supervisionDetails = [
  {
    id: "1",
    bezeichnung: "Studienzeit Mathematik",
    ort: "110",
    einschreibungen: [
      {
        start: new Date(0, 0, 0, 12, 0),
        ende: new Date(0, 0, 0, 12, 20),
        student: student3,
        verified: 1
      },
      {
        start: new Date(0, 0, 0, 12, 0),
        ende: new Date(0, 0, 0, 12, 20),
        student: student1,
        verified: 0
      }
    ]
  }
]

export const otiaDates = [
  {
    label: "Montag, 10.02.2025 | H-Woche",
    code: "2025-02-10",
    disabled: false,
  },
  {
    label: "2025 KW 08 (H): Freitag",
    code: "2025-02-14",
    disabled: true,
  },
  {
    label: "2025 KW 09 (H): Freitag",
    code: "2025-02-19",
    disabled: false,
  }
]

export const blockOptions = [
  {
    label: "13:30 - 14:45",
    block: 1
  },
  {
    label: "15:00 - 16:15",
    block: 2
  }
]

export const otia = [
  {
    bezeichnung: "Studienzeit Mathematik",
    beschreibung: "Hallo Welt",
    auslastung: 0.5,
    maxEinschreibungen: 10,
    tutor: teacher1,
    ort: "110",
    kategorien: ["0", "0-1"],
    id: "1"
  },
  {
    bezeichnung: "Schüler unterrichten Schüler",
    auslastung: 0.8,
    maxEinschreibungen: 5,
    tutor: teacher2,
    ort: "109",
    kategorien: ["0", "0-2"],
    id: "2"
  },
  {
    bezeichnung: "Übungsraum Musik",
    auslastung: 1,
    maxEinschreibungen: 10,
    tutor: null,
    ort: '323',
    kategorien: ["4"],
    id: "3"
  },
  {
    bezeichnung: "Test",
    auslastung: 0,
    maxEinschreibungen: 0,
    tutor: teacher1,
    id: "4",
    kategorien: ["3"],
  },
  {
    bezeichnung: "Test",
    auslastung: 0.5,
    maxEinschreibungen: 0,
    tutor: teacher2,
    id: "5",
    kategorien: ["7"],
  }
]
