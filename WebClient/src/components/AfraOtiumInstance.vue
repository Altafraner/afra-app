<script setup>
import {Card, DataTable, Column, Button, DatePicker} from "primevue";
import {ref, defineProps} from "vue";
import AfraOtiumAnwesenheit from "@/components/AfraOtiumAnwesenheit.vue";

const props = defineProps({
  otium: Object
})

const otium = ref(props.otium)
const isEditing = ref(false)

const formatDate = date => date.toLocaleDateString('de-DE', {
  weekday: "short",
  day: "2-digit",
  month: "2-digit",
});

const padString = (text) => String(text).padStart(2, '0')

const formatTime = date => padString(date.getHours()) + ":" + padString(date.getMinutes());
const formatTutor = tutor => tutor.last_name + ", " + tutor.given_name
const formatStudent = student => student.given_name + " " + student.last_name
</script>

<template>
  <Card>
    <template #title>
      {{ otium.title }}
    </template>
    <template #subtitle>
      {{ formatDate(otium.date) }}, {{ otium.start }} Uhr bis {{ otium.end }} Uhr
    </template>
    <template #content v-if="!isEditing">
      <p>
        Tutor: {{ formatTutor(otium.tutor) }}
      </p>

      <strong class="text-xl font-bold">Einschreibungen</strong>
      <DataTable :value="otium.enrollments" edit-mode="cell">
        <Column header="Schüler:in">
          <template #body="slotProps">
            {{ formatTutor(slotProps.data.student) }}
          </template>
        </Column>
        <Column v-for="field in ['start', 'end']" :field="field" :header="field==='start' ? 'Anfang' : 'Ende'">
          <template #body="{data}">
            {{formatTime(data[field])}}
          </template>
          <template #editor="editorProps">
            <DatePicker time-only v-model="editorProps.data[field]"/>
          </template>
        </Column>
        <Column header="Anwesenheit">
          <template #body="slotProps">
            <afra-otium-anwesenheit :initial-on="slotProps.data.verified" />
          </template>
        </Column>
        <Column class="afra-col-action">
          <template #header>
            <Button aria-label="Schüler:in einschreiben" icon="pi pi-plus" size="small"></Button>
          </template>
          <template #body>
            <span class="inline-flex gap-1">
              <Button aria-label="Ansehen" severity="danger" size="small" variant="text"
                      icon="pi pi-times"></Button>
            </span>
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>

</style>
