<script setup>
import { Button, FloatLabel, InputGroup, Select } from 'primevue';
import InputGroupAddon from 'primevue/inputgroupaddon';
import { formatDate } from '@/helpers/formatters.js';

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
    showLabel: Boolean,
    label: {
        type: String,
        default: 'Datum',
        required: false,
    },
});

const emit = defineEmits(['dateChanged', 'today']);
const emitToday = () => emit('today');

const date = defineModel();

function change_date(next) {
    let n = props.options.findIndex((element) => element.datum === date.value.datum);
    if (n === -1) return;
    n = next(n);
    while (n < props.options.length && n >= 0) {
        if (props.options[n].disabled) {
            n = next(n);
            continue;
        }
        date.value = props.options[n];
        emit('dateChanged');
        return;
    }
}

const increment_date = () => change_date((n) => n + 1);
const decrement_date = () => change_date((n) => n - 1);

function date_to_label(data) {
    return new Date(data.datum);
}
</script>

<template>
    <InputGroup>
        <input-group-addon>
            <Button
                severity="secondary"
                rounded
                icon="pi pi-chevron-left"
                variant="text"
                @click="decrement_date"
            />
        </input-group-addon>
        <FloatLabel variant="on">
            <Select
                id="datum"
                v-model="date"
                :name="name"
                :options="props.options"
                option-disabled="disabled"
                @change="() => emit('dateChanged')"
            >
                <template #value="{ value }">
                    <template v-if="value">
                        {{ formatDate(date_to_label(value)) }} | {{ value.wochentyp }}
                    </template>
                </template>
                <template #option="{ option }"
                    >{{ formatDate(date_to_label(option)) }} |
                    {{ option.wochentyp }}
                </template>
                <template #empty> Kein Datum verfügbar. </template>
            </Select>
            <label v-if="showLabel" for="datum">{{ label }}</label>
        </FloatLabel>
        <input-group-addon v-if="!hideToday">
            <Button
                severity="secondary"
                rounded
                icon="pi pi-calendar-times"
                variant="text"
                aria-label="Heute"
                @click="emitToday"
            />
        </input-group-addon>
        <input-group-addon>
            <Button
                severity="secondary"
                rounded
                icon="pi pi-chevron-right"
                variant="text"
                @click="increment_date"
            />
        </input-group-addon>
    </InputGroup>
</template>

<style scoped></style>
