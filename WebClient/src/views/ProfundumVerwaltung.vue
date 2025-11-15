<template>
  <div class="p-4">
    <h2 class="mb-4">Profundum-Verwaltung</h2>

    <Dropdown
      v-model="selectedStudent"
      :options="mittelstufe"
      optionLabel="name"
      placeholder="Wähle einen Schüler"
      class="w-full md:w-30rem mb-2"
      @change="loadBewertungen"
    />
    
    <Card
      v-for="p in profundas"
      :key="p.instanzId"
      class="mb-4"
    >
      <template #title>{{ p.profundumName }}</template>

      <template #content>
        <DataTable
          :value="p.bewertungen"
          dataKey="kriteriumId"
          class="p-datatable-sm"
        >
          <Column field="name" header="Kriterium"></Column>

          <Column field="grad" header="Bewertung">
            <template #body="{ data }">
              <Rating v-model="data.grad" :cancel="false" />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
    <Button
      label="Speichern"
      icon="pi pi-save"
      :loading="saving"
      @click="saveBewertungen"
    ></Button>    

    <Toast />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { DataTable, Column, Rating, Dropdown, Button, Card, Toast } from 'primevue'
import { useToast } from 'primevue/usetoast'
import axios from 'axios'

const mittelstufe = ref([])
const selectedStudent = ref(null)
const kriterien = ref([])
const bewertungen = ref([])
const profundas = ref([])
const saving = ref(false)
const toast = useToast()

onMounted(async () => {
  try {
    const [peopleRes, kriterienRes] = await Promise.all([
      axios.get('/api/people'),
      axios.get('/api/profundum/bewertung/kriterien')
    ])

    const students = peopleRes.data
      .filter(p => p.rolle?.toLowerCase() === 'mittelstufe')
      .map(s => ({
        ...s,
        name: s.name ?? `${s.vorname} ${s.nachname}`.trim()
      }))

    mittelstufe.value = students

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
    const profundaRes = await axios.get(`/api/profundum/bewertung/${selectedStudent.value.id}/profunda`)

    const items = []

    for (const p of profundaRes.data) {
      const bewRes = await axios.get(`/api/profundum/bewertung/${selectedStudent.value.id}/${p.instanzId}`)

      const bew = kriterien.value.map(k => {
        const existing = bewRes.data.find(b => b.kriteriumId === k.kriteriumId)
        return {
          kriteriumId: k.kriteriumId,
          name: k.name,
          grad: existing?.grad ?? 0
        }
      })

      items.push({
        instanzId: p.instanzId,
        profundumName: p.profundumName,
        bewertungen: bew
      })
    }

    profundas.value = items
  } catch {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Bewertungen konnten nicht geladen werden', life: 3000 })
  }
}


async function saveBewertungen() {
  if (!selectedStudent.value || !profundas.value.length) return
  saving.value = true

  try {
    const payload = []

    for (const p of profundas.value) {
      for (const b of p.bewertungen) {
        payload.push({
          KriteriumId: b.kriteriumId,
          InstanzId: p.instanzId,
          Grad: b.grad
        })
      }
    }

    await axios.post('/api/profundum/bewertung/', payload)

    toast.add({
      severity: 'success',
      summary: 'Gespeichert',
      detail: 'Feedback erfolgreich gespeichert',
      life: 3000
    })
  } catch (e) {
    toast.add({
      severity: 'error',
      summary: 'Fehler',
      detail: e.response?.data ?? 'Fehler beim Speichern',
      life: 3000
    })
  } finally {
    saving.value = false
  }
}

</script>
