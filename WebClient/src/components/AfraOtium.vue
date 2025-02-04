<script setup>
import {ref, defineProps, reactive} from "vue";
import {
  Card,
  Divider,
  Button,
  Tag,
  InputText,
  Message,
  Textarea,
  MultiSelect,
  ToggleSwitch,
  Accordion,
  AccordionContent,
  AccordionPanel,
  AccordionHeader,
  Panel
} from "primevue";
import AfraOtiumDateTable from "@/components/AfraOtiumDateTable.vue";
import AfraOtiumRegTable from "@/components/AfraOtiumRegTable.vue";
import {Form} from '@primevue/forms';
import IftaLabel from "primevue/iftalabel";
import AfraOtiumManagerTable from "@/components/AfraOtiumManagerTable.vue";

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
  designation: otium.value.designation,
  description: otium.value.description,
  tags: otium.value.tags,
  isCataloged: otium.value.isCataloged
})

const tagsAvailable = ref([
  "Studienzeit",
  "Mathe",
  "Deutsch",
  "Physik",
  "Schüler:innen unterrichten Schüler:innen",
  "Wettbewerb"
])
</script>

<template>
  <Card>
    <template #title>
      <span class="flex justify-between align-center">
        {{ otium.designation }}
        <Button v-if="mayEdit && !isEditing" icon="pi pi-pencil"
                @click="toggleEdit"></Button>
      </span>
    </template>
    <template #subtitle v-if="!isEditing">
      <span class="inline-flex gap-1">
        <Tag v-if="mayEdit && otium.isCataloged" value="Im Katalog" severity="success"/>
        <Tag v-if="mayEdit && !otium.isCataloged" value="Nicht Katalogisiert" severity="danger"/>
        <Tag v-for="tag in otium.tags" :value="tag" severity="secondary"></Tag>
      </span>
    </template>
    <template #content v-if="!isEditing">
      <p v-if="!props.minimal" v-for="desc in otium.description.split('\n').filter(desc => desc)">
        {{ desc }}</p>

      <Accordion v-if="!props.minimal" multiple value="">
        <AccordionPanel value="0">
          <AccordionHeader>Termine</AccordionHeader>
          <AccordionContent>
            <afra-otium-date-table v-if="!props.hideDates" :dates="otium.dates"
                                   :allow-enrollment="mayEnroll" :allowEdit="mayEdit"/>
          </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="1">
          <AccordionHeader>Regelmäßigkeiten</AccordionHeader>
          <AccordionContent>
            <afra-otium-reg-table v-if="!props.hideRegularities" :regs="otium.regularities"
                                  :allowEdit="mayEdit"/>
          </AccordionContent>
        </AccordionPanel>
        <AccordionPanel value="2" v-if="mayEdit">
          <AccordionHeader>Verwaltende</AccordionHeader>
          <AccordionContent>
            <afra-otium-manager-table v-if="!props.hideRegularities" :managers="otium.managers"/>
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
          <div class="flex justify-between">
            <label for="ot-katalog">Otium im Katalog anzeigen</label>
            <ToggleSwitch name="isCataloged"/>
          </div>
          <Message size="small" severity="warn" variant="simple" icon="pi pi-exclamation-triangle"
                   v-if="$form['isCataloged'] && !$form['isCataloged'].value">
            Wenn du das Otium nicht katalogisierst, können die Schüler:innen sich nicht selbst
            einschreiben.
          </Message>
        </div>
        <div class="flex flex-col gap-2">
          <label for="ot-designation">Bezeichnung</label>
          <InputText name="designation" id="ot-designation"></InputText>
          <Message size="small" severity="secondary" variant="simple">
            So wird das Otium in allen Listen bezeichnet.
          </Message>
        </div>
        <div class="flex flex-col gap-2">
          <label for="ot-description">Beschreibung</label>
          <Textarea name="description" id="ot-description" rows="5" class="resize-none"
                    auto-resize></Textarea>
          <Message size="small" severity="secondary" variant="simple">
            Die Beschreibung gibt den Schüler:innen mehr Infos.
          </Message>
        </div>
        <div class="flex flex-col gap-2">
          <label for="ot-tags">Tags</label>
          <MultiSelect name="tags" :options="tagsAvailable" id="ot-tags" filter placeholder="Tags"
                       display="chip"></MultiSelect>
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
            das Otium aus dem Katalog zu nehmen.</p>
          <Button severity="danger" label="Löschen" icon="pi pi-trash" variant="outlined"/>
        </Panel>
      </Form>
    </template>
  </Card>
</template>

<style scoped>

</style>
