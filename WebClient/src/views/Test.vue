<script setup>
import {ref} from "vue";
import AfraOtium from "@/components/Otium/AfraOtium.vue";
import AfraOtiumInstance from "@/components/Otium/AfraOtiumInstance.vue";

const otium = ref({
  bezeichnung: "Studienzeit Mathematik",
  kategorien: ["Studienzeit", "Mathe"],
  beschreibung: "Dieses Angebot erlaubt den Schüler:innen und Schülern Hilfe zu mathematischen Aufgabenstellungen zu erhalten. \n\n Es ist ein freiwilliges Angebot, dass alle Schüler:innen und Schüler wahrnehmen können.",
  termine: [
    {
      datum: new Date(),
      start: "11:50",
      ende: "12:30",
      tutor: {
        vorname: "Homer",
        nachname: "Simpson",
      },
    },
    {
      datum: new Date(),
      start: "11:50",
      ende: "12:30",
      tutor: {
        vorname: "Homer",
        nachname: "Simpson",
      },
    }],
  wiederholungen: [
    {
      wochentyp: "N",
      wochentag: "Dienstag",
      start: "11:50",
      ende: "12:30",
      tutor: {
        vorname: "Homer",
        nachname: "Simpson"
      }
    },
    {
      wochentyp: "N",
      wochentag: "Donnerstag",
      start: "10:20",
      ende: "13:00",
      tutor: {
        vorname: "Marge",
        nachname: "Simpson"
      }
    }
  ],
  verwaltende: [
    {
      vorname: "Marge",
      nachname: "Simpson"
    },
    {
      vorname: "Homer",
      nachname: "Simpson"
    }
  ]
})

const instance = ref({
  bezeichnung: "Mathe Studienzeit",
  start: "11:50",
  ende: "12:30",
  datum: new Date(),
  tutor: {
    vorname: "Homer",
    nachname: "Simpson"
  },
  einschreibungen: [
    {
      start: new Date(0,0,0,12,0),
      ende: new Date(0,0,0,12,20),
      student: {
        vorname: "Maggie",
        nachname: "Simpson"
      },
      verified: 1
    },
    {
      start: new Date(0,0,0,12,0),
      ende: new Date(0,0,0,12,20),
      student: {
        vorname: "Maggie",
        nachname: "Simpson"
      },
      verified: 0
    }
  ]
})
</script>

<template>
  <div class="flex justify-center">
    <div class="this_container">
      <h1>Otium</h1>
      <afra-otium :otium="otium" may-enroll may-edit/>
      <afra-otium-instance :otium="instance" />
    </div>
  </div>
</template>

<style scoped>
  .this_container{
    width: 50rem;
  }
</style>
