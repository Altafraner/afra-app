<script setup>
import {computed, ref} from "vue";
import EinwahlSelectorGroup from "@/Profundum/components/EinwahlSelectorGroup.vue";
import {Button} from "primevue";

const options = ref([
  {
    label: "Q1 - Dienstag",
    id: "q1-di",
    options: [
      {label: "Option 1", value: "option1di1"},
      {label: "Option 2", value: "option1di2"},
      {label: "Option 3", value: "option1di3"},
      {label: "Option 4", value: "option1di4", alsoIncludes: ["q1-do"]},
      {label: "Option 5", value: "option1di5"}
    ]
  },
  {
    label: "Q1 - Donnerstag",
    id: "q1-do",
    options: [
      {label: "Option 1", value: "option1do1"},
      {label: "Option 2", value: "option1do2"},
      {label: "Option 3", value: "option1do3"},
      {label: "Option 4", value: "option1do4"},
      {label: "Option 5", value: "option1do5"}
    ]
  },
  {
    label: "Q2 - Dienstag",
    id: "q2-di",
    options: [
      {label: "Option 1", value: "option2di1"},
      {label: "Option 2", value: "option2di2"},
      {label: "Option 3", value: "option2di3"},
      {label: "Option 4", value: "option2di4"},
      {label: "Option 5", value: "option2di5"}
    ]
  },
  {
    label: "Q2 - Donnerstag",
    id: "q2-do",
    options: [
      {label: "Option 1", value: "option2do1"},
      {label: "Option 2", value: "option2do2"},
      {label: "Option 3", value: "option2do3"},
      {label: "Option 4", value: "option2do4"},
      {label: "Option 5", value: "option2do5"}
    ]
  }
]);
const results = ref({});

for (const option of options.value) {
  results.value[option.id] = [null, null, null];
}

function send() {
  console.log('Sending, ...', results);
}

const preSelected = computed(() => {
  // return an array of the ids of all selected options
  const computedResult = {};
  for (const option of options.value) {
    for (let i = 0; i < results.value[option.id].length; i++) {
      const value = results.value[option.id][i];
      const valueObj = option.options.find(opt => opt.value === value);
      if (valueObj && valueObj.alsoIncludes && valueObj.alsoIncludes.length > 0) {
        for (const alsoIncludedElement of valueObj.alsoIncludes) {
          if (!computedResult[alsoIncludedElement]) computedResult[alsoIncludedElement] = [];
          computedResult[alsoIncludedElement][i] = valueObj;
        }
      }
    }
  }
  return computedResult;
})

const maySend = computed(() => {
  for (const option of options.value) {
    if (!results.value[option.id].every(value => value !== null)) {
      return false;
    }
  }
  return true;
});
</script>

<template>
  <h1>Profundums-Einwahl</h1>
  <div v-for="option in options" :key="option.id" class="mb-4">
    <h2>{{ option.label }}</h2>
    <EinwahlSelectorGroup v-model="results[option.id]" :options="option.options"
                          :pre-selected="preSelected[option.id]"/>
  </div>
  <Button :disabled="!maySend" class="mb-4" fluid label="Wünsche abgeben" @click="send"/>
</template>

<style scoped>

</style>
