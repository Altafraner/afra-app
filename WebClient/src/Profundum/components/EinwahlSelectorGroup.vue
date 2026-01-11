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
    conflicts: {
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
    { immediate: true },
);

function forcedAt(i) {
    return props.preSelected?.[i] ?? null;
}
function conflictAt(i) {
    return props.conflicts?.[i] ?? null;
}
</script>

<template>
    <div class="flex flex-col gap-4">
        <div
            v-for="(label, i) in ['Erstwunsch', 'Zweitwunsch', 'Drittwunsch']"
            :key="i"
            class="flex flex-col gap-1"
        >
            <p v-if="conflictAt(i)" class="text-sm text-red-600">
                ⚠ Konflikt: Mehrere zusammenhängende Profunda streuen in diesen Slot:
                <strong>
                    {{
                        conflictAt(i)
                            .map((x) => x.label)
                            .join(', ')
                    }}
                </strong>
            </p>
            <EinwahlSelector
                v-else
                v-model="model[i]"
                :options="optionsCleaned[i]"
                :forced="forcedAt(i)"
                :conflict="conflictAt(i)"
                :label="label"
            />
        </div>
    </div>
</template>

<style scoped></style>
