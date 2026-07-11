<script lang="ts" setup>
import { computed, reactive } from 'vue';
import { formatStudent } from '@/helpers/formatters';
import { AttendanceEvent } from '@/Attendance/models/attendance';
import { UserInfoMinimal } from '@/models/user/user';
import type { FormError, FormSubmitEvent } from '@nuxt/ui';

const props = defineProps<{
    angebote: AttendanceEvent[];
    canMoveNow: boolean;
    student: UserInfoMinimal;
}>();

const emit = defineEmits<{
    close:
        | []
        | [
              {
                  all: boolean;
                  destination: string | undefined;
              },
          ];
}>();

const options = computed(() => {
    return props.angebote.map((angebot) => ({
        label: angebot.location + ' – ' + angebot.name,
        value: angebot.eventId,
    }));
});

const state = reactive<FormSchema>({
    destination: undefined,
    all: undefined,
});

function submit(event: FormSubmitEvent<typeof state>) {
    if (!event.data.destination || event.data.all === undefined) return;
    emit('close', {
        destination: event.data.destination,
        all: event.data.all ?? true,
    });
}

interface FormSchema {
    destination: string | undefined;
    all: boolean | undefined;
}

function validate(state: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (!state.destination)
        errors.push({ name: 'destination', message: 'Bitte wählen Sie ein Ziel aus.' });

    return errors;
}
</script>

<template>
    <UModal title="Schüler:in verschieben">
        <template #description>
            Sie versuchen
            <span class="font-medium">{{ formatStudent(student) }}</span> in ein anderes Otium
            zu verschieben.
        </template>
        <template #footer>
            <div class="text-muted text-sm">
                <p>Durch das Verschieben wird die Anwesenheit auf Abwesend zurückgesetzt.</p>
                <p class="mt-2">
                    Verschieben Sie die Schüler:in ab jetzt und nicht für den gesamten Slot,
                    wird die Belegung beider Slots dokumentiert. Sollten sie die Schüler:in
                    dabei zwischen Angeboten verschiedener Kategorien verschieben, wird keine
                    der Kategorien als belegt gewertet.
                </p>
            </div>
        </template>
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField
                    class="w-full"
                    label="Zielort"
                    required
                    size="lg"
                    name="destination"
                >
                    <USelectMenu
                        v-model="state.destination"
                        :items="options"
                        class="w-full"
                        placeholder="Zielort wählen"
                        value-key="value"
                    />
                </UFormField>

                <UButton
                    :color="canMoveNow ? 'secondary' : 'primary'"
                    label="Ganzen Slot verschieben"
                    type="submit"
                    @click="state.all = true"
                />

                <UButton
                    v-if="canMoveNow"
                    label="Ab jetzt verschieben"
                    type="submit"
                    @click="state.all = false"
                />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
