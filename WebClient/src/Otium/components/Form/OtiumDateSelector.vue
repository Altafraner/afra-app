<script setup>
import { formatDate } from '@/helpers/formatters';
import { computed } from 'vue';

const props = defineProps({
    options: Array,
    hideToday: {
        type: Boolean,
        default: false,
    },
    name: {
        type: String,
        required: false,
    },
    label: {
        type: String,
        default: 'Datum',
        required: false,
    },
    fullSize: {
        type: Boolean,
        default: false,
        required: false,
    },
});

const emit = defineEmits(['dateChanged', 'today']);
const emitToday = () => emit('today');

const date = defineModel();

function change_date(next) {
    let n = props.options.findIndex((element) => element.datum === date.value);
    if (n === -1) return;
    n = next(n);
    while (n < props.options.length && n >= 0) {
        if (props.options[n].disabled) {
            n = next(n);
            continue;
        }
        date.value = props.options[n].datum;
        emit('dateChanged');
        return;
    }
}

const selectedOption = computed(() => {
    if (!date.value) return undefined;
    return props.options.find((o) => o.datum === date.value);
});

const increment_date = () => change_date((n) => n + 1);
const decrement_date = () => change_date((n) => n - 1);

function date_to_label(data) {
    return new Date(data.datum);
}
</script>

<template>
    <UFieldGroup :class="{ 'w-full': fullSize }">
        <UButton
            aria-label="Vorheriger Tag"
            color="neutral"
            icon="i-lucide-chevron-left"
            size="lg"
            variant="outline"
            @click="decrement_date"
        />
        <USelect
            v-model="date"
            :items="options"
            :name="name"
            :placeholder="label"
            :ui="{ base: 'w-full', trailingIcon: 'text-default' }"
            color="neutral"
            value-key="datum"
        >
            <template #item="{ item }">
                <span class="inline-flex gap-2 justify-between w-full md:justify-start">
                    <span>
                        {{ formatDate(date_to_label(item)) }}
                    </span>
                    <UBadge
                        :label="item.wochentyp"
                        color="secondary"
                        size="sm"
                        variant="soft"
                    />
                </span>
            </template>
            <template v-if="selectedOption" #default>
                <span
                    class="inline-flex gap-2 justify-between w-full md:justify-start text-default"
                >
                    <span>
                        {{ formatDate(date_to_label(selectedOption)) }}
                    </span>
                    <UBadge
                        :label="selectedOption.wochentyp"
                        color="secondary"
                        size="sm"
                        variant="soft"
                    />
                </span>
            </template>
        </USelect>
        <UButton
            v-if="!hideToday"
            aria-label="Heute auswählen"
            color="neutral"
            icon="i-lucide-calendar-x"
            size="lg"
            variant="outline"
            @click="emitToday"
        />
        <UButton
            aria-label="Nächster Tag"
            color="neutral"
            icon="i-lucide-chevron-right"
            size="lg"
            variant="outline"
            @click="increment_date"
        />
    </UFieldGroup>
</template>

<style scoped></style>
