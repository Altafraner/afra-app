const padString = (text, n) => String(text).padStart(n, '0')
export const formatTutor = tutor => tutor ? tutor.vorname + ", " + tutor.nachname : ''

export const formatStudent = student => student.vorname + " " + student.nachname

export const formatDate = date => date.toLocaleDateString('de-DE', {
  weekday: "short",
  day: "2-digit",
  month: "short"
});

export const formatTime = date => padString(date.getHours(), 2) + ":" + padString(date.getMinutes(), 2);

