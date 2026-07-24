<script lang="ts" setup>
import { reactive } from 'vue';
import { FormError, FormSubmitEvent } from '@nuxt/ui';
import { UserInfoMinimal } from '@/models/user/user';

interface FormSchema {
    student: string | undefined;
    all: boolean | undefined;
}

const props = defineProps<{
    canMoveNow: boolean;
}>();

const emit = defineEmits<{
    close: [FormSchema];
}>();

function validate(state: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (!state.student) {
        errors.push({ name: 'student', message: 'Bitte wählen Sie eine Person aus.' });
    }

    return errors;
}

function submit(event: FormSubmitEvent<FormSchema>) {
    if (event.data.student === undefined || event.data.all === undefined) return;
    emit('close', {
        student: event.data.student,
        all: event.data.all,
    });
}

const state = reactive<FormSchema>({ all: undefined, student: undefined });
</script>

<template>
    <UModal
        description="Hier können Sie eine Schüler:in in das aktuelle Angebot verschieben."
        title="Schüler:in hierhin verschieben"
    >
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField class="w-full" label="Schüler:in" name="student" required>
                    <PersonSelector
                        v-model="state.student"
                        :filter="(student: UserInfoMinimal) => student.rolle === 'Mittelstufe'"
                        class="w-full"
                        hideRolle
                        placeholder="Schüler:in auswählen"
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
                    color="primary"
                    label="Ab jetzt verschieben"
                    type="submit"
                    @click="state.all = false"
                />
            </UForm>
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
    </UModal>
</template>

<style scoped></style>
