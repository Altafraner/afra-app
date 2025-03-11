<script setup>
import {Button} from "primevue";
import {useUser} from "@/stores/useUser.js";
import {formatStudent} from "../helpers/formatters.js";
const user = useUser()
</script>

<template>
  <h1>Dashboard</h1>
  <p>Hallo {{formatStudent(user.user)}}, schön, dass du hier her gefunden hast. </p>
  <p>Du siehst hier eine erste Testversion der Otiumsverwaltung. Aktuell ist nur ein Teil der Oberfläche sichtbar. Es werden nur Testdaten verwendet, wundere dich daher bitte nicht, wenn Änderungen nicht gespeichert werden oder alle Links zum gleichen Otium führen.</p>
  <h2>Arbeitsstand</h2>
  <p>Du kannst aktuell den <RouterLink to="/Katalog">Katalog</RouterLink> betrachteten, wie ihn ein:e Schüler:in sehen würde. Außerdem ist die <RouterLink to="/aufsicht">Aufsichtsansicht</RouterLink> wie eine Lehrer:in sie sehen würde verfügbar. Unter <RouterLink to="/test">Test</RouterLink> findest du außerdem einige Komponenten, wie sie Lehrer:innen sehen werden.</p>
  <p>Es fehlt noch das <RouterLink to="/Dashboard">Dashboard</RouterLink> für Lehrer:innen und Schüler:innen inklusive der Ansichten für Mentees und verwaltete Otia. Außerdem fehlen die Ansichten für den Otiums-Verantwortlichen und die Schulleitung.</p>
  <div class="flex flex-row gap-5 flex-wrap">
    <Button as="RouterLink" to="/dashboard" label="Dashboard Schüler:in" />
    <Button as="RouterLink" to="/teacher" label="Dashboard Lehrer:in" />
    <Button as="RouterLink" to="/katalog" label="Katalog" />
    <Button as="RouterLink" to="/test" label="Test" />
    <Button as="RouterLink" to="/aufsicht" label="Aufsicht" />
  </div>
</template>

<style scoped>
p a {
  color: var(--p-primary-500);
}

p a:visited {
  color: var(--p-primary-700);
}
</style>
