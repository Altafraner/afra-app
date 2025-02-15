<script setup>
import {ref} from "vue";
import AfraOtium from "@/components/AfraOtium.vue";
import AfraOtiumInstance from "@/components/AfraOtiumInstance.vue";

const otium = ref({
  isCataloged: true,
  designation: "Studienzeit Mathematik",
  tags: ["Studienzeit", "Mathe"],
  description: "Dieses Angebot erlaubt den Schüler:innen und Schülern Hilfe zu mathematischen Aufgabenstellungen zu erhalten. \n\n Es ist ein freiwilliges Angebot, dass alle Schüler:innen und Schüler wahrnehmen können.",
  dates: [
    {
      date: new Date(),
      start: "11:50",
      end: "12:30",
      tutor: {
        given_name: "Homer",
        last_name: "Simpson",
      },
    },
    {
      date: new Date(),
      start: "11:50",
      end: "12:30",
      tutor: {
        given_name: "Homer",
        last_name: "Simpson",
      },
    }],
  regularities: [
    {
      week_type: "N",
      day_of_week: "Dienstag",
      start: "11:50",
      end: "12:30",
      tutor: {
        given_name: "Homer",
        last_name: "Simpson"
      }
    },
    {
      week_type: "N",
      day_of_week: "Donnerstag",
      start: "10:20",
      end: "13:00",
      tutor: {
        given_name: "Marge",
        last_name: "Simpson"
      }
    }
  ],
  managers: [
    {
      given_name: "Marge",
      last_name: "Simpson"
    },
    {
      given_name: "Homer",
      last_name: "Simpson"
    }
  ]
})

const instance = ref({
  title: "Mathe Studienzeit",
  start: "11:50",
  end: "12:30",
  date: new Date(),
  tutor: {
    given_name: "Homer",
    last_name: "Simpson"
  },
  enrollments: [
    {
      start: new Date(0,0,0,12,0),
      end: new Date(0,0,0,12,20),
      student: {
        given_name: "Maggie",
        last_name: "Simpson"
      },
      verified: 1
    },
    {
      start: new Date(0,0,0,12,0),
      end: new Date(0,0,0,12,20),
      student: {
        given_name: "Maggie",
        last_name: "Simpson"
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
      <afra-otium :otium="otium" may-enroll/>
      <afra-otium-instance :otium="instance" />
    </div>
  </div>
</template>

<style scoped>
  .this_container{
    width: 50rem;
  }
</style>
