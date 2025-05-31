<script setup>
import {computed, inject, ref} from "vue";
import Form from "@primevue/forms/form";
import {formatStudent} from "@/helpers/formatters.js";
import FloatLabel from "primevue/floatlabel";
import {Message, Select, SplitButton} from "primevue";

const dialogRef = inject("dialogRef");

const destination = ref(null);
const all = ref(undefined);
const form = ref();

const options = computed(() => {
  return dialogRef.value.data.angebote.map(angebot => ({
    label: angebot.ort + ' – ' + angebot.otium,
    value: angebot.terminId
  }));
})

const buttonOptions = [
  {
    label: "Ganzen Block verschieben",
    command: moveAll
  }
];

function resolve({values}) {
  const errors = {};

  if (!values.destination) {
    errors.destination = [{message: "Bitte wählen Sie ein Ziel aus."}];
  }

  return {values, errors};
}

function save() {
  all.value = false;
  triggerSubmit()
}

function moveAll() {
  all.value = true;
  triggerSubmit()
}

function triggerSubmit() {
  form.value.$el.dispatchEvent(new Event("submit", {
    bubbles: true,
    cancelable: true
  }));
}

function submit({valid}) {
  if (!valid) return;
  dialogRef.value.close({
    destination: destination.value,
    all: all.value
  })
}
</script>

<template>
  <Form ref="form" v-slot="$form" :resolver="resolve" class="flex flex-col gap-3" @submit="submit">
    <p>
      Sie versuchen <span class="font-bold">{{ formatStudent(dialogRef.data.student) }}</span> in
      ein anderes Otium zu verschieben.
      Durch das Verschieben wird die Anwesenheit der Schüler:in nicht verändert.
    </p>
    <div class="w-full">
      <FloatLabel variant="on">
        <Select id="destination" v-model="destination" :options="options" fluid
                name="destination" option-label="label" option-value="value"/>
        <label for="destination">Zielort</label>
      </FloatLabel>
      <Message v-if="$form.destination?.invalid" severity="error" size="small"
               variant="simple">
        {{ $form.destination.error.message }}
      </Message>
    </div>
    <SplitButton :model="buttonOptions" class="mt-3" fluid label="Ab jetzt verschieben"
                 @click="save"/>
  </Form>
</template>

<style scoped>

</style>
