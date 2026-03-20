<script setup>
import { computed, inject, ref } from 'vue';
import Form from '@primevue/forms/form';
import { Button, Message } from 'primevue';
import PersonSelector from '@/components/PersonSelector.vue';

const dialogRef = inject('dialogRef');

const all = ref(undefined);
const form = ref();

const canMoveNow = computed(() => {
    return dialogRef.value.data.canMoveNow;
});

const buttonOptions = [
    {
        label: 'Ganzen Block verschieben',
        command: moveAll,
    },
];

function resolve({ values }) {
    const errors = {
        student: [],
    };

    if (!values.student) {
        errors.student.push('Bitte wählen Sie eine Person aus.');
    }

    return { values, errors };
}

function save() {
    all.value = false;
    triggerSubmit();
}

function moveAll() {
    all.value = true;
    triggerSubmit();
}

function triggerSubmit() {
    form.value.$el.dispatchEvent(
        new Event('submit', {
            bubbles: true,
            cancelable: true,
        }),
    );
}

function submit({ valid, values }) {
    if (!valid) return;
    console.log(values);
    dialogRef.value.close({
        student: values.person,
        all: all.value,
    });
}
</script>

<template>
    <Form
        ref="form"
        v-slot="$form"
        :resolver="resolve"
        class="flex flex-col gap-3"
        @submit="submit"
    >
        <div class="w-full">
            <PersonSelector
                :filter="(student) => student.rolle === 'Mittelstufe'"
                hideRolle
                name="person"
            >
                <template #label>Schüler:in</template>
            </PersonSelector>
            <Message
                v-if="$form.person?.invalid"
                severity="error"
                size="small"
                variant="simple"
            >
                {{ $form.person.error }}
            </Message>
        </div>
        <p>Durch das Verschieben wird die Anwesenheit auf Abwesend zurückgesetzt.</p>
        <div
            :class="{
                'grid-cols-2': canMoveNow,
                'grid-cols-1': !canMoveNow,
            }"
            class="grid gap-3 mt-3"
        >
            <Button
                :severity="canMoveNow ? 'secondary' : 'primary'"
                label="Ganzen Slot verschieben"
                @click="moveAll"
            />
            <Button
                v-if="canMoveNow"
                label="Ab jetzt verschieben"
                severity="primary"
                @click="save"
            />
        </div>
    </Form>
</template>

<style scoped></style>
