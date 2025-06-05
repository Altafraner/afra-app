<script setup>
import {ref} from "vue";
import {Form} from "@primevue/forms";
import {Button, FloatLabel, InputText, Message, Textarea} from "primevue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import {useSettings} from "@/stores/useSettings.js";

const emits = defineEmits(["submit"]);

const settings = useSettings()

const loading = ref(false);
const bezeichnung = ref(null);
const beschreibung = ref(null);
const kategorie = ref(null);

function resolver({ values }) {
  console.log(values);
  
  const errors = {};

  const bezeichnung = (values.bezeichnung ?? '').trim();
  const beschreibung = (values.beschreibung ?? '').trim();

  if (!bezeichnung || bezeichnung === '') {
    errors.bezeichnung = [{  message: 'Bezeichnung ist erforderlich.' }];
  } else if ([...bezeichnung].length < 3) {
    errors.bezeichnung = [{  message: 'Bezeichnung muss über 3 Zeichen besitzen.' }];
  } else if ([...bezeichnung].length > 10) {
    errors.bezeichnung = [{ message: 'Bezeichnung muss unter 10 Zeichen besitzen.' }];
  }

  if (!beschreibung || beschreibung === '') {
    errors.beschreibung = [{  message: 'Beschreibung ist erforderlich.' }];
  } else if ([...beschreibung].length < 3) {
    errors.beschreibung = [{  message: 'Beschreibung muss über 3 Zeichen besitzen.' }];
  } else if ([...beschreibung].length > 500) {
    errors.beschreibung = [{ message: 'Beschreibung muss unter 500 Zeichen besitzen.' }];
  }

  if (!values.kategorie) {
    errors.kategorie = [{ message: 'Kategorie ist erforderlich.' }];
  }

  console.log(errors);
  
  return { values, errors };
}


function submit({valid, values}) {
  if (!valid) return;
  const kategorieId = Object.keys(values.kategorie)[0];
  emits('submit', {
    bezeichnung: values.bezeichnung,
    beschreibung: values.beschreibung,
    kategorie: kategorieId
  });
}

async function setup() {
  loading.value = true
  await settings.updateKategorien()
  loading.value = false
}

setup()

</script>

<template>
  <Form v-if="!loading" v-slot="$form" :resolver="resolver" class="flex flex-col gap-3" @submit="submit">
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <InputText id="bezeichnung" v-model="bezeichnung" name="bezeichnung" fluid />
        <label for="bezeichnung">Bezeichnung</label>
      </FloatLabel>
      <Message v-if="$form.bezeichnung?.invalid" severity="error" size="small" variant="simple">
        {{ $form.bezeichnung.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <Textarea id="beschreibung" v-model="beschreibung" name="beschreibung" auto-resize fluid rows="2"/>
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
    <Button class="mt-4" label="Erstellen" severity="primary" type="submit" />
  </Form>
</template>

<style scoped>

</style>
