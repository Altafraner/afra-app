<script setup>
import {ref} from "vue";
import {Form} from "@primevue/forms";
import {Button, FloatLabel, InputText, Message, Textarea} from "primevue";
import {useOtiumStore} from "@/Otium/stores/otium.js";
import AfraKategorySelector from "@/Otium/components/Form/AfraKategorySelector.vue";

const emits = defineEmits(["submit"]);

const settings = useOtiumStore()

const loading = ref(false);
const bezeichnung = ref(null);
const beschreibung = ref(null);
const kategorie = ref(null);

function resolver({values}) {
  const errors = {}

  if (!values.bezeichnung || values.bezeichnung.length < 1)
    errors.bezeichnung = [{message: "Es muss eine Bezeichnung gesetzt sein"}]
  if (values.bezeichnung && values.bezeichnung.length > 70)
    errors.bezeichnung = [{message: "Die Bezeichnung darf maximal 70 Zeichen lang sein"}]
  if (!values.beschreibung || values.beschreibung.length < 1)
    errors.beschreibung = [{message: "Es muss eine Beschreibung gesetzt sein"}]
  if (values.beschreibung && values.beschreibung.length > 500)
    errors.beschreibung = [{message: "Die Beschreibung darf maximal 500 Zeichen lang sein"}]
  if (!values.kategorie)
    errors.kategorie = [{message: "Es muss eine Kategorie ausgewählt sein"}]

  return {values, errors}
}

function submit({valid}) {
  if (!valid) return;

  const kategorieId = Object.keys(kategorie.value)[0]
  emits('submit', {
    bezeichnung: bezeichnung.value,
    beschreibung: beschreibung.value,
    kategorie: kategorieId
  })
}

async function setup() {
  loading.value = true
  await settings.updateKategorien()
  loading.value = false
}

setup()

</script>

<template>
  <Form v-if="!loading" v-slot="$form" :resolver="resolver" class="flex flex-col gap-3"
        @submit="submit">
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <InputText id="bezeichnung" v-model="bezeichnung" fluid name="bezeichnung"/>
        <label for="bezeichnung">Bezeichnung</label>
      </FloatLabel>
      <Message v-if="$form.bezeichnung?.invalid" severity="error" size="small" variant="simple">
        {{ $form.bezeichnung.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <Textarea id="beschreibung" v-model="beschreibung" auto-resize fluid name="beschreibung"
                  rows="2"/>
        <label for="beschreibung">Beschreibung</label>
      </FloatLabel>
      <Message v-if="$form.beschreibung?.invalid" severity="error" size="small" variant="simple">
        {{ $form.beschreibung.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <AfraKategorySelector id="kategorie" v-model="kategorie" :options="settings.kategorien"
                              fluid hide-clear name="kategorie" placeholder=""/>
        <label for="kategorie">Kategorie</label>
      </FloatLabel>
      <Message v-if="$form.kategorie?.invalid" severity="error" size="small" variant="simple">
        {{ $form.kategorie.error.message }}
      </Message>
    </div>
    <Button class="mt-4" label="Erstellen" severity="primary" type="submit"/>
  </Form>
</template>

<style scoped>

</style>
