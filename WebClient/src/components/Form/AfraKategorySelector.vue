<script setup>
import {ref} from "vue";
import {TreeSelect} from "primevue";

const props = defineProps({
  options: Array,
  name: String,
  hideClear: Boolean
})

const emit = defineEmits(["change"])

const kategorie = defineModel()
console.log(props.options)
const optionsTree = ref(convertToTreeSelectOptions(props.options))
console.log(optionsTree.value)


function convertToTreeSelectOptions(options) {
  return options.map(treeMappingFunction);
}

function treeMappingFunction(element) {
  console.log(element)
  return {
    key: element.id,
    label: element.bezeichnung,
    afra_icon: element.icon ?? null,
    color: element.cssColor ?? null,
    children: element.children ? convertToTreeSelectOptions(element.children) : null
  }
}
</script>

<template>
  <TreeSelect :name="props.name" placeholder="Kategorie" :options="optionsTree" v-model="kategorie" @change="() => emit('change')" :show-clear="!props.hideClear">
    <template #option="slotProps" >
      <div class="flex gap-1 align-center">
        <span v-if="slotProps.node.afra_icon" :class="`ot-angebot-icon p-tree-node-icon ${slotProps.node.color ? 'ot-angebot-white' : ''}`" :style="`background-color: ${slotProps.node.color ?? 'unset'}`">
          <i :class="slotProps.node.afra_icon"/>
        </span>
        <span>
          {{slotProps.node.label}}
        </span>
      </div>
    </template>
  </TreeSelect>
</template>

<style scoped>
.ot-angebot-icon {
  font-size: 0.8em;
  width: 2em;
  height: 2em;
  border-radius: 1em;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.ot-angebot-icon i{
  font-size: 1.1em;
}

.ot-angebot-white{
  color: #fff;
}
</style>
