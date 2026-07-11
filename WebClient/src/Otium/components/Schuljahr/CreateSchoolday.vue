<script setup>
import { inject, ref } from 'vue';
import FloatLabel from 'primevue/floatlabel';
import DatePicker from 'primevue/datepicker';
import { Button, Message, Select } from 'primevue';
import Form from '@primevue/forms/form';
import { formatMachineDate } from '@/helpers/formatters';
import { mande } from 'mande';

const dialogRef = inject('dialogRef');
const emit = defineEmits(['update']);
const toast = useToast();

const date = ref(null);
const wochentyp = ref(null);
const loading = ref(false);

function resolve({ values }) {
    const errors = {};

    if (!values.date) errors.date = [{ message: 'Bitte geben Sie ein Datum an.' }];

    if (!values.wochentyp)
        errors.wochentyp = [{ message: 'Bitte geben Sie den Wochentyp an.' }];

    return { values, errors };
}

async function trySubmit({ valid }) {
    if (!valid) return;
    if (!dialogRef.value.data || !('initialValues' in dialogRef.value.data)) {
        await submit();
        return;
    }
    loading.value = true;
    await submit();
}

async function submit() {
    loading.value = true;
    const data = [];

    date.value.setHours(12);

    data.push({
        datum: formatMachineDate(date.value),
        wochentyp: wochentyp.value,
        blocks: [],
    });

    const api = mande('/api/management/schuljahr');
    try {
        await api.post(data);
        toast.add({
            color: 'success',
            title: 'Erfolg',
            description: 'Der Termin wurde erfolgreich gespeichert.',
        });
        emit('update');
        dialogRef.value.close();
    } catch (error) {
        console.error(error);
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Die Termine konnten nicht gespeichert werden.',
        });
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <Form v-slot="$form" :resolver="resolve" class="flex flex-col gap-4" @submit="trySubmit">
        <div class="w-full">
            <FloatLabel variant="on">
                <DatePicker
                    id="date"
                    v-model="date"
                    date-format="dd.mm.yy"
                    fluid
                    name="date"
                    select-other-months
                    show-icon
                />
                <label for="date">Datum</label>
            </FloatLabel>
            <Message v-if="$form.date?.invalid" severity="error" size="small" variant="simple">
                {{ $form.date.error.message }}
            </Message>
        </div>
        <div class="w-full">
            <FloatLabel variant="on">
                <Select
                    id="wochentyp"
                    v-model="wochentyp"
                    :options="['H-Woche', 'N-Woche']"
                    fluid
                    name="wochentyp"
                />
                <label for="wochentyp">Wochentyp</label>
            </FloatLabel>
            <Message
                v-if="$form.wochentyp?.invalid"
                severity="error"
                size="small"
                variant="simple"
            >
                {{ $form.wochentyp.error.message }}
            </Message>
        </div>
        <Button :loading="loading" class="mt-4" fluid label="Abschließen" type="submit" />
    </Form>
</template>

<style scoped></style>
