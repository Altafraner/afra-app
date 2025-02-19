<script setup>
import {ref, reactive} from "vue";
import {
  Card,
  Divider,
  Button,
  Tag,
  InputText,
  Message,
  Textarea,
  Accordion,
  AccordionContent,
  AccordionPanel,
  AccordionHeader,
  Panel
} from "primevue";
import AfraOtiumDateTable from "@/components/Otium/AfraOtiumDateTable.vue";
import AfraOtiumRegTable from "@/components/Otium/AfraOtiumRegTable.vue";
import {Form} from '@primevue/forms';
import AfraOtiumManagerTable from "@/components/Otium/AfraOtiumManagerTable.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import {kategorien} from "@/helpers/testdata.js";
import AfraKategorieTag from "@/components/Otium/AfraKategorieTag.vue";

const props = defineProps({
  otium: Object,
  hideDates: Boolean,
  hideRegularities: Boolean,
  mayEnroll: Boolean,
  mayEdit: Boolean,
  minimal: Boolean
})
const otium = ref(props['otium'])
const isEditing = ref(false)

if (props.minimal && (props.mayEdit || props.mayEnroll))
  console.error("Minimal is not allowed along with allowEnrollment and allowEdit.")

function toggleEdit() {
  isEditing.value = !isEditing.value
}

const initialValues = reactive({
  bezeichnung: otium.value.bezeichnung,
  beschreibung: otium.value.beschreibung,
  kategorien: otium.value.kategorien,
})

function findKategorie(id, kategorien){
  const index = kategorien.findIndex((e) => e.id === id);
  if (index!==-1) {
    return kategorien[index]
  }

  for (const kategorie of kategorien ?? []) {
    const childResult = findKategorie(id, kategorie.children)
    if (childResult != null) return childResult
  }

  return null
}

const kategorieOptionsTree = ref(kategorien)
</script>

<template>
  <Card>
    <template #title>
      <span class="flex justify-between align-center">
        {{ otium.bezeichnung }}
        <Button v-if="mayEdit && !isEditing" icon="pi pi-pencil"
                @click="toggleEdit"></Button>
      </span>
    </template>
    <template #subtitle v-if="!isEditing">
      <span class="inline-flex gap-1">
        <AfraKategorieTag v-for="tag in otium.kategorien" :value="findKategorie(tag, kategorien)" severity="secondary"></AfraKategorieTag>
      </span>
    </template>
    <template #content v-if="!isEditing">
      <p v-if="!props.minimal" v-for="beschreibung in otium.beschreibung.split('\n').filter(desc => desc)">
        {{ beschreibung }}</p>

      <Accordion v-if="!props.minimal" multiple value="">
        <AccordionPanel value="0">
          <AccordionHeader>Termine</AccordionHeader>
          <AccordionContent>
            <afra-otium-date-table v-if="!props.hideDates" :dates="otium.termine"
                                   :allow-enrollment="mayEnroll" :allowEdit="mayEdit"/>
          </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="1">
          <AccordionHeader>Regelmäßigkeiten</AccordionHeader>
          <AccordionContent>
            <afra-otium-reg-table v-if="!props.hideRegularities" :regs="otium.wiederholungen"
                                  :allowEdit="mayEdit"/>
          </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="2" v-if="mayEdit">
          <AccordionHeader>Verwaltende</AccordionHeader>
          <AccordionContent>
            <afra-otium-manager-table v-if="!props.hideRegularities" :managers="otium.verwaltende"/>
          </AccordionContent>
        </AccordionPanel>
      </Accordion>


    </template>
    <template #content v-if="isEditing">
      <Form v-slot="$form" :initial-values="initialValues" @submit="onFormSubmit"
            class="flex flex-col gap-5">
        <Divider/>

        <div class="font-bold text-xl">Eigenschaften</div>
        <div class="flex flex-col gap-2">
          <label for="ot-designation">Bezeichnung</label>
          <InputText name="bezeichnung" id="ot-designation"></InputText>
          <Message size="small" severity="secondary" variant="simple">
            So wird das Otium in allen Listen bezeichnet.
          </Message>
        </div>
        <div class="flex flex-col gap-2">
          <label for="ot-description">Beschreibung</label>
          <Textarea name="beschreibung" id="ot-description" rows="5" class="resize-none"
                    auto-resize></Textarea>
          <Message size="small" severity="secondary" variant="simple">
            Die Beschreibung gibt den Schüler:innen mehr Infos.
          </Message>
        </div>
        <div class="flex flex-col gap-2">
          <label for="ot-tags">Kategorie</label>
          <AfraKategorySelector name="kategorien" :options="kategorieOptionsTree" id="ot-tags" hide-clear></AfraKategorySelector>
          <Message size="small" severity="secondary" variant="simple">
            Gib Tags an, um den Schüler:innen zu helfen, dein Otium zu finden.
          </Message>
        </div>
        <Divider/>

        <div class="flex gap-2 width-fill justify-stretch">
          <Button type="submit" label="Speichern" fluid/>
          <Button severity="secondary" label="Abbrechen" @click="toggleEdit"/>
        </div>

        <Divider/>
        <Panel toggleable collapsed>
          <template #header>
            <span class="text-xl font-bold danger">
              <i class="pi pi-exclamation-triangle"/>
              Otium Löschen
            </span>
          </template>
          <p>Wenn du das Otium Löschen möchtest, kannst du das hier tun. </p>
          <p>
            <strong>Bitte beachte, dass du ein Otium nur dann Löschen kannst, wenn es keine (auch
              vergangenen) Termine mehr zu diesem Otium gibt.</strong> Versuche alternativ
            alle kommenden Termine zu löschen.</p>
          <Button severity="danger" label="Löschen" icon="pi pi-trash" variant="outlined"/>
        </Panel>
      </Form>
    </template>
  </Card>
</template>

<style scoped>

</style>
