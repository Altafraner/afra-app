<script setup>
import {computed, ref} from "vue";
import EinwahlSelectorGroup from "@/Profundum/components/EinwahlSelectorGroup.vue";
import {Button} from "primevue";
import {testdata} from "@/Profundum/components/testdata.js";

const options = ref(testdata);
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
