<script setup>
import {inject, ref} from "vue";
import FloatLabel from "primevue/floatlabel";
import DatePicker from "primevue/datepicker";
import {Button, Message, MultiSelect, Select, useToast} from "primevue";
import Form from "@primevue/forms/form";
import {formatMachineDate} from "@/helpers/formatters.js";
import {mande} from "mande";

const dialogRef = inject('dialogRef');
const emit = defineEmits(['update'])
const toast = useToast();

const date = ref(null);
const wochentyp = ref(null);
const blocks = ref([]);
const loading = ref(false);

const blocksAvailable = ref([
  {label: 'Block 1', value: '1'},
  {label: 'Block 2', value: '2'},
  {label: 'Abendsport', value: 'P'},
])

function resolve({values}) {
  const errors = {}

  if (!values.date)
    errors.date = [{message: 'Bitte geben Sie ein Datum an.'}]

  if (!values.wochentyp)
    errors.wochentyp = [{message: 'Bitte geben Sie den Wochentyp an.'}]

  if (!values.blocks || values.blocks.length === 0)
    errors.blocks = [{message: 'Bitte legen sie die stattfindenden Blöcke fest.'}]

  return {values, errors}
}

async function submit({valid}) {
  if (!valid) return;
  const data = [];

  date.value.setHours(12);

  data.push({
    datum: formatMachineDate(date.value),
    wochentyp: wochentyp.value,
    blocks: blocks.value
  });

  const api = mande('/api/schuljahr/schultage')
  try {
    await api.post(data);
    toast.add({
      severity: 'success',
      summary: 'Erfolg',
      detail: 'Der Termin wurden erfolgreich gespeichert.',
      life: 15000
    });
    emit('update');
    dialogRef.value.close();
  } catch (error) {
    console.error(error)
    toast.add({
      severity: 'error',
      summary: 'Fehler',
      detail: 'Die Termine konnten nicht gespeichert werden.',
    });
  }
}
</script>

<template>
  <Form v-slot="$form" :resolver="resolve" class="flex flex-col gap-4"
        @submit="submit">
    <div class="w-full">
      <FloatLabel variant="on">
        <DatePicker id="date" v-model="date" date-format="dd.mm.yy"
                    fluid name="date" select-other-months show-icon/>
        <label for="date">Datum</label>
      </FloatLabel>
      <Message v-if="$form.date?.invalid" severity="error" size="small"
               variant="simple">
        {{ $form.date.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel variant="on">
        <Select id="wochentyp"
                v-model="wochentyp"
                :options="['H-Woche', 'N-Woche']" fluid name="wochentyp"/>
        <label for="wochentyp">Wochentyp</label>
      </FloatLabel>
      <Message v-if="$form.wochentyp?.invalid" severity="error" size="small"
               variant="simple">
        {{ $form.wochentyp.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel variant="on">
        <MultiSelect id="blocks" v-model="blocks" :options="blocksAvailable"
                     :showToggleAll="false" fluid name="blocks" option-label="label"
                     option-value="value"/>
        <label for="blocks">Blöcke</label>
      </FloatLabel>
      <Message v-if="$form.blocks?.invalid" severity="error" size="small"
               variant="simple">
        {{ $form.blocks.error.message }}
      </Message>
    </div>
    <Button :loading="loading" class="mt-4" fluid label="Abschließen" type="submit"/>
  </Form>
</template>

<style scoped>

</style>
