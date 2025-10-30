<template>
  <div class="p-4">
    <h2 class="mb-4">Profundum-Verwaltung</h2>

    <Dropdown
      v-model="selectedStudent"
      :options="students"
      optionLabel="name"
      placeholder="Wähle einen Schüler"
      class="w-full md:w-30rem mb-2"
      @change="loadBewertungen"
    />
    
    <Card v-if="selectedStudent && kriterien.length">
      <template #title>{{ selectedStudent.name }}</template>
      <template #content>
        <DataTable :value="bewertungen" dataKey="kriteriumId" class="p-datatable-sm">
          <Column field="name" header="Kriterium"></Column>
          <Column field="grad" header="Bewertung">
            <template #body="{ data }">
              <Rating v-model="data.grad" :cancel="false" />
            </template>
          </Column>
        </DataTable>

        <div class="text-right mt-4">
          <Button
            label="Speichern"
            icon="pi pi-save"
            :loading="saving"
            @click="saveBewertungen"
          />
        </div>
      </template>
    </Card>

    <Toast />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { DataTable, Column, Rating, Dropdown, Button, Card, Toast } from 'primevue'
import { useToast } from 'primevue/usetoast'
import axios from 'axios'

const students = ref([])
const selectedStudent = ref(null)
const kriterien = ref([])
const bewertungen = ref([])
const saving = ref(false)
const toast = useToast()

onMounted(async () => {
  try {
    const [peopleRes, kriterienRes] = await Promise.all([
      axios.get('/api/people'),
      axios.get('/api/profundum/bewertung/kriterien')
    ])

    students.value = peopleRes.data.map(s => ({
      ...s,
      name: s.name ?? `${s.vorname} ${s.nachname}`
    }))

    kriterien.value = kriterienRes.data.map(k => ({
      kriteriumId: k.id,
      name: k.bezeichnung
    }))
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Daten konnten nicht geladen werden', life: 3000 })
  }
})

async function loadBewertungen() {
  if (!selectedStudent.value) return

  try {
    const res = await axios.get(`/api/profundum/bewertung/${selectedStudent.value.id}`)
    bewertungen.value = kriterien.value.map(k => {
      const existing = res.data.find(b => b.kriteriumId === k.kriteriumId)
      return {
        ...k,
        grad: existing?.grad ?? 0
      }
    })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Bewertungen konnten nicht geladen werden', life: 3000 })
  }
}

async function saveBewertungen() {
  if (!selectedStudent.value || !bewertungen.value.length) return
  saving.value = true

  try {
    const payload = bewertungen.value.map(b => ({
      KriteriumId: b.kriteriumId,
      InstanzId: selectedStudent.value.id,
      Grad: b.grad
    }))
    await axios.post('/api/profundum/bewertung/', payload)
    toast.add({ severity: 'success', summary: 'Gespeichert', detail: 'Feedback erfolgreich gespeichert', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: e.response?.data ?? 'Fehler beim Speichern', life: 3000 })
  } finally {
    saving.value = false
  }
}
</script>
