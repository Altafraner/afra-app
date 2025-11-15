<template>
  <div class="p-4">
    <h2 class="mb-4">Kriterium-Verwaltung</h2>

    <div class="mb-4 flex gap-2">
      <InputText v-model="newBezeichnung" placeholder="Neue Kriterium Bezeichnung" class="flex-1" />
      <Button label="Erstellen" icon="pi pi-plus" @click="createKriterium" :loading="saving" />
    </div>

    <Card>
      <template #title>Alle Kriterien</template>
      <template #content>
        <DataTable :value="kriterien" dataKey="id" class="p-datatable-sm">
          <Column field="bezeichnung" header="Bezeichnung"></Column>
          <Column header="Aktionen">
            <template #body="{ data }">
              <Button
                icon="pi pi-pencil"
                class="p-button-warning p-button-sm mr-2"
                @click="openRenameDialog(data)"
              />
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

    <Dialog
      v-model:visible="renameDialogVisible"
      header="Kriterium umbenennen"
      :modal="true"
      :closable="false"
      style="width: 40rem; max-width: 90vw"
    >
      <div class="flex flex-col gap-3">
        <label for="renameInput" class="font-medium">Neue Bezeichnung:</label>
        <InputText id="renameInput" v-model="renameBezeichnung" autofocus />
        <div class="flex justify-end gap-2 mt-3">
          <Button label="Abbrechen" severity="secondary" @click="renameDialogVisible = false" />
          <Button label="Speichern" icon="pi pi-check" @click="confirmRename" :loading="savingRename" />
        </div>
      </div>
    </Dialog>
    <Dialog
      v-model:visible="deleteDialogVisible"
      header="Kriterium löschen"
      :modal="true"
      :closable="false"
      style="width: 40rem; max-width: 90vw"
    >
      <div class ="flex flex-col gap-3">
        <p>Um zu bestätigen das sie dieses Kriterium und somit auch alle deren Einträge löschen wollen, geben sie den Kriterienname ein:</p>
        <div class="flex justify-end gap-2 mt-3">
          <InputText v-model="deleteKriteriumText" placeholder="Geben sie den Kriterienname ein." style="width:70%"/>
          <Button label="Abbrechen" severity="secondary" @click="deleteDialogVisible = false" />
          <Button label="Löschen" icon="pi pi-trash" class="p-button-danger" @click="confirmDelete(deleteKriteriumDialogId)" />
        </div>
      </div>
    </Dialog>
    <Toast />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { InputText, Button, Card, DataTable, Column, Dialog, Toast } from 'primevue'
import { useToast } from 'primevue/usetoast'
import axios from 'axios'

const kriterien = ref([])
const newBezeichnung = ref('')
const saving = ref(false)
const savingRename = ref(false)
const renameDialogVisible = ref(false)
const renameBezeichnung = ref('')
const deleteDialogVisible = ref(false)
const deleteKriteriumDialogId = ref(null)
const deleteKriteriumText = ref(null)
const selectedKriterium = ref(null)
const toast = useToast()

async function loadKriterien() {
  try {
    const res = await axios.get('/api/profundum/bewertung/kriterien')
    kriterien.value = res.data
  } catch {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Kriterien konnten nicht geladen werden', life: 3000 })
  }
}

async function createKriterium() {
  if (!newBezeichnung.value.trim()) {
    toast.add({ severity: 'warn', summary: 'Warnung', detail: 'Bezeichnung darf nicht leer sein', life: 3000 })
    return
  }
  saving.value = true
  try {
    const res = await axios.post('/api/profundum/bewertung/kriterium/create', newBezeichnung.value, {
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

function openRenameDialog(kriterium) {
  selectedKriterium.value = kriterium
  renameBezeichnung.value = kriterium.bezeichnung
  renameDialogVisible.value = true
}

async function confirmRename() {
  if (!renameBezeichnung.value.trim() || !selectedKriterium.value) return
  savingRename.value = true
  try {
    await axios.post(
      `/api/profundum/bewertung/kriterien/rename/${selectedKriterium.value.id}`,
      JSON.stringify(renameBezeichnung.value),
      { headers: { 'Content-Type': 'application/json' } }
    )
    const kriterium = kriterien.value.find(k => k.id === selectedKriterium.value.id)
    if (kriterium) kriterium.bezeichnung = renameBezeichnung.value
    toast.add({ severity: 'success', summary: 'Umbenannt', detail: 'Kriterium erfolgreich umbenannt', life: 3000 })
    renameDialogVisible.value = false
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: e.response?.data ?? 'Fehler beim Umbenennen', life: 3000 })
  } finally {
    savingRename.value = false
  }
}

function openDeleteDialog(id) {
  deleteKriteriumDialogId.value = id
  deleteDialogVisible.value = true
}

async function deleteKriterium(id) {
  openDeleteDialog(id)
}

//this is subject to change, I mainly got issues with the text tho, maybe add like a warning or whtevr
async function confirmDelete(id) {
  if(deleteKriteriumText.value !== (kriterien.value.find(k => k.id === id)?.bezeichnung || '')) {
    toast.add({ severity: 'warn', summary: 'Warnung', detail: 'Kriterienname stimmt nicht überein', life: 3000 })
    return
  }
  try {
    await axios.delete(`/api/profundum/bewertung/kriterien/delete/${id}`)
    kriterien.value = kriterien.value.filter(k => k.id !== id)
    toast.add({ severity: 'success', summary: 'Gelöscht', detail: 'Kriterium erfolgreich gelöscht', life: 3000 })
    deleteDialogVisible.value = false
    deleteKriteriumText.value = null
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: e.response?.data ?? 'Fehler beim Löschen', life: 3000 })
    deleteDialogVisible.value = false
    deleteKriteriumText.value = null
  }
}

onMounted(loadKriterien)
</script>
