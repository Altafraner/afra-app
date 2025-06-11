<script setup>
import {formatDate} from "@/helpers/formatters.js";
import {
  Accordion,
  AccordionContent,
  AccordionHeader,
  AccordionPanel,
  Badge,
  Button,
  Column,
  DataTable,
  Message
} from "primevue";
import {useUser} from "@/stores/user.js";
import {computed} from "vue";
import {useOtiumStore} from "@/Otium/stores/otium.js";
import {findPath} from "@/helpers/tree.js";
import AfraKategorieTag from "@/Otium/components/Shared/AfraKategorieTag.vue";
import {marked} from "marked";

const props = defineProps({
  termine: Array,
  showKatalog: Boolean,
  student: {
    type: Object,
    required: false
  }
})
const user = useUser();
const otiumStore = useOtiumStore()

const findKategorie = (kategorie) => {
  const path = findPath(otiumStore.kategorien, kategorie);
  for (const element of path) {
    if (element.icon != null) {
      return element;
    }
  }
  return null;
}

const termineMitKategorie = computed(() => {
  return props.termine.map(termin => {
    return {
      ...termin,
      messageHtml: termin.message ? marked(termin.message, {sanitize: true}) : null,
      einschreibungen: termin.einschreibungen.map(einschreibung => {
        return {
          ...einschreibung,
          kategorie: findKategorie(einschreibung.kategorieId)
        }
      })
    }
  })
})

const isOs = computed(() => {
  if (props.student) {
    return props.student.rolle === "Oberstufe";
  }

  return user.user.rolle === "Oberstufe"
})

</script>

<template>
  <Accordion v-if="termine != null">
    <AccordionPanel v-for="termin in termineMitKategorie" :key="termin.monday" :value="termin.monday">
      <AccordionHeader>
        <div class="flex w-full justify-between mr-4">
          <span>
            {{
              formatDate(new Date(termin.monday))
            }} – {{ formatDate(new Date(new Date(termin.monday).getTime() + 518400000)) }}
          </span>
          <span v-if="!isOs" class="flex flex-row gap-3">
            <Badge v-if="!termin.message" class="w-[8rem]"
                   severity="secondary">Ok</Badge>
            <Badge v-else class="w-[8rem]" severity="danger">Offen</Badge>
          </span>
        </div>
      </AccordionHeader>
      <AccordionContent>
        <Message v-if="termin.message && !isOs" class="mb-2" severity="warn">
          <div v-html="termin.messageHtml"/>
        </Message>
        <Message v-else-if="!isOs" class="mb-2" severity="success">
          <div>Die Einschreibungen entsprechen den Vorgaben.</div>
        </Message>
        <DataTable :value="termin.einschreibungen" group-rows-by="datum" row-group-mode="subheader">
          <Column header="Otium">
            <template #body="{data}">
              <AfraKategorieTag v-if="data.kategorie" :value="data.kategorie" hide-name minimal/>
              <Button :label="data.otium" as="RouterLink" class="" variant="text"
                      :to="{name: 'Katalog-Termin', params: {terminId: data.terminId}}"/>
            </template>
          </Column>
          <Column header="Ort">
            <template #body="{data}">
              {{ data.ort }}
            </template>
          </Column>
          <Column header="Datum">
            <template #body="{data}">
              {{ formatDate(new Date(data.datum)) }}
            </template>
          </Column>
          <Column field="block" header="Block">
            <template #body="{data}">
              {{ data.block }}. Block
            </template>
          </Column>
          <template #groupheader="{data}">
            <span class="font-semibold">{{ formatDate(new Date(data.datum)) }}</span>
          </template>
          <template #empty>
            <div class="flex justify-center">
              Keine Einträge
            </div>
          </template>
          <template #footer>
            <div class="flex flex-row justify-between items-center">
              <Button v-if="props.showKatalog" class="w-[8rem]" size="small" as="RouterLink"
                      :to="{name: 'Katalog-Datum', params: {datum: termin.monday}}" label="Katalog"/>
              <span v-else/>
              <span v-if="!isOs"
                    class="flex flex-row gap-3 mr-[var(--p-icon-size)] flex-wrap justify-end">
                <Badge v-if="termin.message" class="w-[8rem]"
                       severity="danger">Unvollständig</Badge>
              </span>
            </div>
          </template>
        </DataTable>
      </AccordionContent>
    </AccordionPanel>
  </Accordion>
</template>

<style scoped>

</style>
