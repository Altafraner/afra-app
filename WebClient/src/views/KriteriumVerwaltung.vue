<template>
  <div class="p-4">
    <h2 class="mb-4">Kriterium-Verwaltung</h2>

    <div class="mb-4 flex gap-2">
      <InputText v-model="newBezeichnung" placeholder="Neue Kriterium Bezeichnung" class="flex-1"/>
      <Button label="Erstellen" icon="pi pi-plus" @click="createKriterium" :loading="saving"/>
    </div>

    <Card>
      <template #title>Alle Kriterien</template>
      <template #content>
        <DataTable :value="kriterien" dataKey="id" class="p-datatable-sm">
          <Column field="bezeichnung" header="Bezeichnung"></Column>
          <Column header="Aktionen">
            <template #body="{ data }">
              <Button
                icon="pi pi-trash"
                class="p-button-danger p-button-sm"
                @click="deleteKriterium(data.id)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Toast />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { InputText, Button, Card, DataTable, Column, Toast } from 'primevue'
import { useToast } from 'primevue/usetoast'
import axios from 'axios'

const kriterien = ref([])
const newBezeichnung = ref('')
const saving = ref(false)
const toast = useToast()

async function loadKriterien() {
  try {
    const res = await axios.get('/api/profundum/bewertung/kriterien')
    kriterien.value = res.data
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Kriterien konnten nicht geladen werden', life: 3000 })
  }
}

async function createKriterium() {
  if (!newBezeichnung.value.trim()) return
  saving.value = true
  try {
    const res = await axios.post('/api/profundum/bewertung/create-kriterium', newBezeichnung.value, {
      headers: { 'Content-Type': 'application/json' }
    })
    kriterien.value.push(res.data)
    newBezeichnung.value = ''
    toast.add({ severity: 'success', summary: 'Erstellt', detail: 'Kriterium erfolgreich erstellt', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: e.response?.data ?? 'Fehler beim Erstellen', life: 3000 })
  } finally {
    saving.value = false
  }
}

async function deleteKriterium(id) {
  try {
    await axios.delete(`/api/profundum/bewertung/kriterien/${id}`)
    kriterien.value = kriterien.value.filter(k => k.id !== id)
    toast.add({ severity: 'success', summary: 'Gelöscht', detail: 'Kriterium erfolgreich gelöscht', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: e.response?.data ?? 'Fehler beim Löschen', life: 3000 })
  }
}

onMounted(loadKriterien)
</script>
