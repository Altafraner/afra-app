const padString = (text, n) => String(text).padStart(n, '0')
export const formatTutor = tutor => tutor ? tutor.vorname + ", " + tutor.nachname : ''

export const formatStudent = student => student.vorname + " " + student.nachname

export const formatDate = date => date.toLocaleDateString('de-DE', {
  weekday: "short",
  day: "2-digit",
  month: "short"
});

export const formatTime = date => padString(date.getHours(), 2) + ":" + padString(date.getMinutes(), 2);

export const chooseColor = (now, max) => {
  if (max===0 || now <= 0.7) return 'var(--p-button-success-background)'
  if (now < 1) return 'var(--p-button-warn-background)'
  return 'var(--p-button-danger-background)'
}
