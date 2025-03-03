<script setup>
import {Button, InputGroup, Select} from "primevue";
import InputGroupAddon from "primevue/inputgroupaddon";
import {ref} from "vue";
import {formatDate} from "../../helpers/formatters.js";

const props = defineProps({
  options: Array
})

const emit = defineEmits(["dateChanged"])

const date = defineModel()

const datesAvailable = ref(props.options)

function change_date(next) {
  let n = datesAvailable.value.findIndex((element) => element.datum === date.value.datum)
  n = next(n)
  while (n < datesAvailable.value.length && n >= 0) {
    if (datesAvailable.value[n].disabled) {
      n = next(n)
      continue
    }
    date.value = datesAvailable.value[n]
    emit("dateChanged")
    return
  }
}

const increment_date = () => change_date((n) => n + 1);
const decrement_date = () => change_date((n) => n - 1);

function date_to_label (data) {
  return new Date(data.datum)
}
</script>

<template>
  <InputGroup>
    <input-group-addon>
      <Button severity="secondary" rounded icon="pi pi-chevron-left" variant="text"
              @click="decrement_date"/>
    </input-group-addon>
    <Select filter v-model="date" option-label="datum" option-disabled="disabled"
            :options="datesAvailable" @change="() => emit('dateChanged')" >
      <template #value="{value}">{{formatDate(date_to_label(value))}} | {{value.wochentyp}}</template>
      <template #option="{option}">{{formatDate(date_to_label(option))}} | {{option.wochentyp}}</template>
    </Select>
    <input-group-addon>
      <Button severity="secondary" rounded icon="pi pi-chevron-right" variant="text"
              @click="increment_date"/>
    </input-group-addon>
  </InputGroup>
</template>

<style scoped>

</style>
