<script setup>
import {inject, ref} from "vue";
import {Button, PickList} from "primevue";
import {formatDate} from "@/helpers/formatters.js";

const dialogRef = inject("dialogRef");

const mappedOptions = dialogRef.value.data.options.map(option => ({
  label: formatDate(new Date(option)),
  id: option
}));

const selection = ref([[...mappedOptions], []]);

function cancel() {
  dialogRef.value.close();
}

function submit() {
  dialogRef.value.close({selected: selection.value[1].map(s => s.id)});
}

</script>

<template>
  <div class="flex flex-col gap-3">
    <p>Bitte wähle alle Termine, zu denen du auch kommen möchtest. Du wirst nur zu den Terminen
      eingeschrieben, zu denen die Einschreibung erlaubt ist.</p>
    <PickList v-model="selection" :show-source-controls="false" :show-target-controls="false"
              data-key="id">
      <template #option="{ option }">
        {{ option.label }}
      </template>
      <template #sourceheader><span class="font-bold text-lg">Verfügbar</span></template>
      <template #targetheader><span class="font-bold text-lg">Ausgewählt</span></template>
    </PickList>
    <div class="sm:grid sm:grid-cols-[auto_1fr] flex flex-wrap justify-stretch gap-3">
      <Button label="Abbrechen" severity="secondary" @click="cancel"/>
      <Button label="Einschreiben" @click="submit"/>
    </div>
  </div>
</template>

<style scoped>

</style>
