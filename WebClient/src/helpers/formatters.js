﻿const wochentage = ['Sonntag', 'Montag', 'Dienstag', 'Mittwoch', 'Donnerstag', 'Freitag', 'Samstag']
const padString = (text, n) => String(text).padStart(n, '0')
export const formatTutor = tutor => tutor ? tutor.nachname + ", " + tutor.vorname : ''

export const formatStudent = student => student.vorname + " " + student.nachname

export const formatPerson = person => person.rolle === "Oberstufe" || person.Rolle === "Mittelstufe" ? formatStudent(person) : formatTutor(person)

export const formatDate = date => date.toLocaleDateString('de-DE', {
  weekday: "short",
  day: "2-digit",
  month: "short"
});

export const formatMachineDate = date => date.toISOString().split('T')[0]

export const formatTime = date => padString(date.getHours(), 2) + ":" + padString(date.getMinutes(), 2);

export const chooseColor = (now, max) => {
  if (max === 0 || now <= 0.7) return 'var(--p-button-success-background)'
  if (now < 1) return 'var(--p-button-warn-background)'
  return 'var(--p-button-danger-background)'
}

export const chooseSeverity = (now) => {
  if (now <= 70) return 'success'
  if (now < 100) return 'warn'
  return 'danger'
}

export const formatDayOfWeek = (number) => wochentage[number % 7]
