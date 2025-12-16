<template>
  <div class="page">
    <h2 class="title">Profundum-Verwaltung</h2>

    <div class="selector-row md:flex gap-4 items-center mb-6">
      <Select
        v-model="selectedStudent"
        :options="mittelstufe"
        optionLabel="name"
        placeholder="Schüler auswählen"
        showClear
        filter
        filterPlaceholder="Suche…"
        appendTo="body"
        class="
          md:flex-1
          backdrop-blur-[6px]
          px-[0.6rem] py-[0.25rem]
          transition-all duration-150
          focus-within:shadow-[0_0_0_2px_rgba(0,132,255,0.35)]
        "
        @change="loadBewertungen"
      />
      <Button
        label="Speichern"
        icon="pi pi-save"
        :loading="saving"
        @click="saveBewertungen"
        class="save-button white-space-nowrap"
      />
    </div>

    <div v-if="profundas.length">
      <Card
        v-for="p in profundas"
        :key="p.instanzId"
        class="profundum-card p-4 rounded-x1"
      >
        <template #title>
          <div class="card-title">{{ p.profundumName }}</div>
        </template>

        <template #content>
          <div v-if="p.kriterien?.length" class="kriterien-list">
            <div
              v-for="k in p.kriterien"
              :key="k.kriteriumId"
              class="kriterium border-[var(--surface-border)] rounded-md overflow-hidden"
            >
              <div
                class="kriterium-header flex justify-between p-3 p-4 cursor-pointer transition-colors duration-150 hover:bg-[var(--surface-hover)]"
                :class="{ disabled: !k.enabled }"
                @click="toggleKriterium(p, k.kriteriumId)"
              >
                <div class="header-left flex flex-col">
                  <div class="k-name">{{ k.name }}</div>
                  <div class="k-sub text-xs opacity-70">Anker bewerten</div>
                </div>

                <div class="header-right flex items-center gap-2">
                  <div class="k-avg text-[0.85rem] opacity-80">Ø {{ formatAvg(k) }}</div>
                </div>
              </div>
              <transition
                enter-active-class="transition-all duration-200 ease-in-out"
                leave-active-class="transition-all duration-200 ease-in-out"
                enter-from-class="opacity-0 -translate-y-[6px]"
                leave-to-class="opacity-0 -translate-y-[6px]"
              >
                <div v-if="k.open" class="anker-table p-2 p-4 p-4 p-4">
                  <DataTable
                    :value="k.anker"
                    dataKey="ankerId"
                    class="p-datatable-sm"
                    :rowClass="rowClass"
                  >
                    <Column field="name" header="Anker" />
                    <Column header="Bewertung">
                      <template #body="{ data }">
                        <div class="rating-container flex gap-2 items-center">
                          <div
                            v-for="n in 5"
                            :key="n"
                            class="rating-circle w-[1.35rem] h-[1.35rem] rounded-full border-2 border-[rgba(0,132,255,0.45)] transition duration-150 hover:scale-[1.12]"
                            :class="{
                              'bg-[rgba(0,132,255,0.9)] border-[rgba(0,132,255,1)] cursor-pointer': Number(data.grad) >= n,
                              'opacity-40 cursor-not-allowed !scale-100': Number(data.grad) === 0
                            }"
                            @click="data.grad === 0 ? null : data.grad = n"
                          />
                        </div>
                      </template>
                    </Column>

                    <Column header="Bewerten?">
                      <template #body="{ data }">
                        <div
                          class="toggle w-[3.4rem] h-[1.55rem] rounded-full bg-[#bbb] relative flex items-center cursor-pointer px-1 transition-all duration-250 justify-start"
                          :class="{ 'bg-[rgba(0,132,255,0.85)]': data.grad > 0 }"
                          @click="data.grad = data.grad > 0 ? 0 : 1"
                        >
                          <div
                            class="toggle-inner w-[1.25rem] h-[1.25rem] bg-white rounded-full transition-transform duration-250"
                            :class="{ 'translate-x-[1.75rem]': data.grad > 0 }"
                          ></div>

                          <span
                            class="absolute -right-[2.2rem] w-[2rem] text-[0.8rem] opacity-80 pointer-events-none text-[var(--text-color)]"
                          ></span>
                        </div>
                      </template>
                    </Column>
                  </DataTable>
                </div>
              </transition>

            </div>
          </div>

          <div v-else class="text-muted opacity-70 text-[0.9rem] p-2">Keine Kriterien verfügbar.</div>
        </template>
      </Card>
    </div>
    <Toast />
  </div>
</template>


<script setup>
import { ref, onMounted } from 'vue'
import { DataTable, Column, Select, Button, Card, Toast } from 'primevue'
import { useToast } from 'primevue/usetoast'
import { mande } from 'mande'

const mittelstufe = ref([])
const selectedStudent = ref(null)
const profundas = ref([])
const saving = ref(false)
const toast = useToast()

const allKriterien = ref([])
const allAnker = ref([])

onMounted(async () => {
  try {
    const peopleRes = await mande('/api/people').get()
    const kriterienRes = await mande('/api/profundum/bewertung/kriterien').get()
    const ankerRes = await mande('/api/profundum/bewertung/anker').get()

    mittelstufe.value = peopleRes
      .filter(p => p.rolle?.toLowerCase() === 'mittelstufe')
      .map(s => ({
        ...s,
        name: s.name ?? `${s.vorname} ${s.nachname}`.trim()
      }))

    allKriterien.value = kriterienRes.map(k => ({
      kriteriumId: k.id,
      name: k.bezeichnung
    }))

    allAnker.value = ankerRes.map(a => ({
      ankerId: a.id,
      name: a.bezeichnung ?? a.name ?? 'Anker'
    }))
  } catch (e) {
    console.log(e)
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Daten konnten nicht geladen werden', life: 3000 })
  }
})

async function loadBewertungen() {
  if (!selectedStudent.value) return

  try {
    const profundaRes = await mande(
      `/api/profundum/bewertung/${selectedStudent.value.id}/profunda`
    ).get()

    const items = []

    for (const p of profundaRes) {
      let existingBew = []
      try {
        const bewRes = await mande(
          `/api/profundum/bewertung/${selectedStudent.value.id}/${p.instanzId}/ankerbewertungen`
        ).get()

        existingBew = Array.isArray(bewRes) ? bewRes : []
      } catch (e) {
        existingBew = []
      }

      const kritArray = allKriterien.value.map(k => {
        const ankerList = allAnker.value.map(a => {
          const found = existingBew.find(b =>
            b.ankerId === a.ankerId &&
            (b.profundumId === p.instanzId || b.ProfundumId === p.instanzId) &&
            (b.kriteriumId === k.kriteriumId || b.kriteriumid === k.kriteriumId)
          )
          return {
            ankerId: a.ankerId,
            name: a.name,
            grad: found?.grad ?? 0
          }
        })

        const enabled = ankerList.some(a => a.grad > 0)

        return {
          kriteriumId: k.kriteriumId,
          name: k.name,
          anker: ankerList,
          enabled,
          open: false
        }
      })

      items.push({
        instanzId: p.instanzId,
        profundumName: p.profundumName,
        kriterien: kritArray
      })
    }

    profundas.value = items
  } catch (e) {
    console.log(e)
    toast.add({
      severity: 'error',
      summary: 'Fehler',
      detail: 'Bewertungen konnten nicht geladen werden',
      life: 3000
    })
  }
}


function rowClass(data) {
  return data.grad > 0 ? '' : 'row-disabled'
}

function formatAvg(k) {
  const vals = k.anker.map(a => Number(a.grad) || 0).filter(v => v > 0)
  if (!vals.length) return '—'
  return (vals.reduce((s, v) => s + v, 0) / vals.length).toFixed(2)
}

function toggleKriterium(p, kriteriumId) {
  const k = p.kriterien.find(x => x.kriteriumId === kriteriumId)
  if (!k) return
  k.open = !k.open
}


async function saveBewertungen() {
  if (!selectedStudent.value || !profundas.value.length) return
  saving.value = true

  try {
    const requests = []

    for (const p of profundas.value) {
      for (const k of p.kriterien) {
        for (const a of k.anker) {
          const grad = Number(a.grad) || 0
          const wirdbewertet = grad > 0

          requests.push(
            mande(
              `/api/profundum/bewertung/${selectedStudent.value.id}/${p.instanzId}/${a.ankerId}/${k.kriteriumId}/${grad}/${wirdbewertet}`
            ).post({})
          )
        }
      }
    }

    await Promise.all(requests)

    toast.add({ severity: 'success', summary: 'Gespeichert', detail: 'Feedback gespeichert', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: e.response?.data ?? 'Fehler beim Speichern', life: 3000 })
    console.log(e)
  } finally {
    saving.value = false
  }
}

</script>
