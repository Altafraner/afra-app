<script setup>
import { ref } from 'vue';
import { TreeSelect } from 'primevue';
import { findPath } from '@/helpers/tree.js';
import AfraKategorieTag from '@/Otium/components/Shared/AfraKategorieTag.vue';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';

const props = defineProps({
    options: Array,
    name: String,
    hideClear: Boolean,
    fluid: Boolean,
    id: String,
    placeholder: {
        type: String,
        default: 'Kategorie',
    },
});

const emit = defineEmits(['change']);

const kategorie = defineModel();
const optionsTree = ref(convertToTreeSelectOptions(props.options));

function convertToTreeSelectOptions(options) {
    return options.map(treeMappingFunction);
}

function treeMappingFunction(element) {
    return {
        key: element.id,
        label: element.bezeichnung,
        afra_icon: element.icon ?? null,
        color: element.cssColor ?? null,
        children: element.children ? convertToTreeSelectOptions(element.children) : null,
    };
}
</script>

<template>
    <TreeSelect
        :id="id"
        v-model="kategorie"
        :fluid="fluid"
        :name="props.name"
        :options="optionsTree"
        :placeholder="placeholder"
        :show-clear="!props.hideClear"
        @change="() => emit('change')"
    >
        <template #option="slotProps">
            <div class="flex gap-1 items-center">
                <span
                    v-if="slotProps.node.afra_icon"
                    :class="`ot-angebot-icon p-tree-node-icon ${slotProps.node.color ? 'ot-angebot-white' : ''}`"
                    :style="`background-color: ${slotProps.node.color ?? 'unset'}`"
                >
                    <i :class="slotProps.node.afra_icon" />
                </span>
                <span>
                    {{ slotProps.node.label }}
                </span>
            </div>
        </template>
        <template #value="{ value }">
            <SimpleBreadcrumb
                v-if="value.length === 1"
                :model="findPath(options, value[0].key)"
            >
                <template #item="{ item }">
                    <AfraKategorieTag :value="item" minimal />
                </template>
            </SimpleBreadcrumb>
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

.ot-angebot-icon i {
    font-size: 1.1em;
}

.ot-angebot-white {
    color: #fff;
}
</style>
