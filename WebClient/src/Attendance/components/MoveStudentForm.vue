<script setup>
import { computed, inject, ref } from 'vue';
import Form from '@primevue/forms/form';
import { formatStudent } from '@/helpers/formatters';
import FloatLabel from 'primevue/floatlabel';
import { Button, Message, Select } from 'primevue';

const dialogRef = inject('dialogRef');

const destination = ref(null);
const all = ref(undefined);
const form = ref();

const options = computed(() => {
    return dialogRef.value.data.angebote.map((angebot) => ({
        label: angebot.location + ' – ' + angebot.name,
        value: angebot.id ?? angebot.eventId,
    }));
});

const canMoveNow = computed(() => {
    return dialogRef.value.data.canMoveNow;
});

function resolve({ values }) {
    const errors = {
        destination: [],
    };

    if (!values.destination) {
        errors.destination.push('Bitte wählen Sie ein Ziel aus.');
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

function submit({ valid }) {
    if (!valid) return;
    dialogRef.value.close({
        destination: destination.value,
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
        <p>
            Sie versuchen
            <span class="font-bold">{{ formatStudent(dialogRef.data.student) }}</span> in ein
            anderes Otium zu verschieben.
        </p>
        <p>Durch das Verschieben wird die Anwesenheit auf Abwesend zurückgesetzt.</p>
        <div class="w-full">
            <FloatLabel variant="on">
                <Select
                    id="destination"
                    v-model="destination"
                    :options="options"
                    fluid
                    name="destination"
                    option-label="label"
                    option-value="value"
                />
                <label for="destination">Zielort</label>
            </FloatLabel>
            <Message
                v-if="$form.destination?.invalid"
                severity="error"
                size="small"
                variant="simple"
            >
                {{ $form.destination.error }}
            </Message>
        </div>
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
