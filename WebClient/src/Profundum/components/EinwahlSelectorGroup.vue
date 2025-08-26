<script setup>
import EinwahlSelector from '@/Profundum/components/EinwahlSelector.vue';
import { computed, ref, watch } from 'vue';

const props = defineProps({
    options: {
        type: Array,
        required: true,
    },
    preSelected: {
        type: Array,
        required: false,
    },
});

const model = defineModel({
    default: [null, null, null],
});

const disabled = ref([false, false, false]);

function cleanOptions(options, ...vars) {
    return options.filter((option) => {
        return !vars.includes(option.value);
    });
}

const optionsCleaned = computed(() => {
    return [
        cleanOptions(props.options, model.value[1], model.value[2]),
        cleanOptions(props.options, model.value[0], model.value[2]),
        cleanOptions(props.options, model.value[0], model.value[1]),
    ];
});

watch(
    () => props.preSelected,
    (newValue, oldValue) => {
        if (newValue) {
            for (const i in newValue) {
                if (newValue[i] !== null) {
                    model.value[i] = newValue[i].value;
                    disabled.value[i] = true;
                }
            }
        }
        for (const i in oldValue) {
            if (!newValue || !newValue[i]) {
                model.value[i] = null;
                disabled.value[i] = false;
            }
        }
    },
);
</script>

<template>
    <div class="flex flex-col gap-3">
        <EinwahlSelector
            v-model="model[0]"
            :disabled="disabled[0]"
            :options="optionsCleaned[0]"
            :pre-selected="preSelected && preSelected[0] ? preSelected[0] : {}"
            label="Erstwunsch"
        />
        <EinwahlSelector
            v-model="model[1]"
            :disabled="disabled[1]"
            :options="optionsCleaned[1]"
            :pre-selected="preSelected && preSelected[1] ? preSelected[1] : {}"
            label="Zweitwunsch"
        />
        <EinwahlSelector
            v-model="model[2]"
            :disabled="disabled[2]"
            :options="optionsCleaned[2]"
            :pre-selected="preSelected && preSelected[2] ? preSelected[2] : {}"
            label="Drittwunsch"
        />
    </div>
</template>

<style scoped></style>
