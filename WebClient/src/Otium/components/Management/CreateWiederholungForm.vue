<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';

import { useOtiumStore } from '@/Otium/stores/otium.js';
import OtiumDateSelector from '@/Otium/components/Form/OtiumDateSelector.vue';
import { UserInfoMinimal } from '@/models/user/user.ts';
import { usePeople } from '@/stores/people.ts';
import { FormError, FormSubmitEvent } from '@nuxt/ui';

const emit = defineEmits(['close']);

const settings = useOtiumStore();
const people = usePeople();
const props = defineProps<{
    initialValues?: {
        ort: string;
        maxEinschreibungen: number | null;
        tutor: UserInfoMinimal | null;
    };
}>();

const hasInitialData = computed(() => {
    return props.initialValues !== undefined;
});

const state = reactive<FormSchema>({
    block: undefined,
    end: undefined,
    hasMaxTn: props.initialValues?.maxEinschreibungen != null,
    hasTutor: props.initialValues?.tutor != null,
    maxTn: props.initialValues?.maxEinschreibungen ?? undefined,
    start: undefined,
    tutor: props.initialValues?.tutor?.id ?? undefined,
    wochentag: undefined,
    wochentyp: undefined,
    location: props.initialValues?.ort ?? undefined,
});

const dates = ref<any[]>([]);
const loading = ref<boolean>(true);

function validate(state: Partial<FormSchema>): FormError[] {
    const errors = [] as FormError[];

    if (!hasInitialData.value) {
        if (!state.wochentyp)
            errors.push({ name: 'wochentyp', message: 'Bitte wählen Sie einen Wochentyp!' });
        if (!state.wochentag)
            errors.push({ name: 'wochentag', message: 'Bitte wählen Sie einen Wochentag!' });
        if (!state.block)
            errors.push({ name: 'block', message: 'Bitte wählen Sie einen Block!' });
        if (!state.start)
            errors.push({ name: 'start', message: 'Bitte wählen Sie einen Startzeitpunkt!' });
        if (!state.end)
            errors.push({ name: 'end', message: 'Bitte wählen Sie einen Endzeitpunkt!' });
    }

    if (!state.location || state.location.length < 1)
        errors.push({ name: 'location', message: 'Bitte geben Sie einen Ort an.' });
    if (state.location && state.location.length > 20)
        errors.push({ name: 'location', message: 'Der Ort darf maximal 20 Zeichen lang sein' });
    if (state.hasMaxTn && !state.maxTn)
        errors.push({
            name: 'maxTn',
            message: 'Bitte geben Sie eine maximale Anzahl an Teilnehmenden an!',
        });
    if (state.hasTutor && !state.tutor)
        errors.push({
            name: 'tutor',
            message: 'Bitte wählen Sie einen Tutor aus!',
        });

    return errors;
}

async function getTermine() {
    await settings.updateSchuljahr();
    if (!settings.schuljahr) return;
    dates.value = settings.schuljahr as any[];
}

async function setup() {
    const personPromise = people.updatePersonen();
    const terminePromise = getTermine();
    const blocksPromise = settings.updateBlocks();

    await Promise.all([personPromise, terminePromise, blocksPromise]);
    loading.value = false;
}

const datesAvailable = computed(() => {
    if (!state.wochentyp || !state.wochentag || !state.block) return [];

    const now = new Date(new Date().toDateString());
    const result = dates.value.filter((date) => {
        const datum = new Date(date.datum);
        return (
            date.blocks.some((b: any) => b.schemaId === (state.block ?? '')) &&
            datum >= now &&
            datum.getDay() === state.wochentag &&
            date.wochentyp === state.wochentyp
        );
    });
    console.log(result);
    return result;
});

watch(datesAvailable, (newValue) => {
    console.log(newValue);
    if (newValue.length == 0) {
        state.start = undefined;
        state.end = undefined;
        return;
    }
    if (!newValue.some((d) => d.datum === state.start)) {
        state.start = newValue[0].datum;
    }
    if (!newValue.some((d) => d.datum === state.end)) {
        state.end = newValue[newValue.length - 1].datum;
    }
});

function submit(event: FormSubmitEvent<FormSchema>) {
    const result = {
        wochentyp: event.data.wochentyp ?? null,
        wochentag: event.data.wochentag ?? null,
        von: event.data.start ?? null,
        bis: event.data.end ?? null,
        block: event.data.block ?? null,
        ort: event.data.location,
        person: event.data.tutor ?? null,
        maxEnrollments: event.data.maxTn ?? null,
    };
    emit('close', result);
}

watch(
    () => state.hasMaxTn,
    (newValue) => {
        if (!newValue) {
            state.maxTn = undefined;
        }
    },
);
watch(
    () => state.hasTutor,
    (newValue) => {
        if (!newValue) {
            state.tutor = undefined;
        }
    },
);

setup();
</script>

<script lang="ts">
interface FormSchema {
    wochentyp: string | undefined;
    wochentag: number | undefined;
    block: string | undefined;
    start: string | undefined;
    end: string | undefined;
    hasTutor: boolean;
    hasMaxTn: boolean;
    tutor: string | undefined;
    maxTn: number | undefined;
    location: string | undefined;
}

const wochentagOptions = [
    {
        label: 'Montag',
        value: 1,
    },
    {
        label: 'Dienstag',
        value: 2,
    },
    {
        label: 'Mittwoch',
        value: 3,
    },
    {
        label: 'Donnerstag',
        value: 4,
    },
    {
        label: 'Freitag',
        value: 5,
    },
    {
        label: 'Samstag',
        value: 6,
    },
];
</script>

<template>
    <UModal
        :description="
            hasInitialData
                ? 'Ändern Sie mehrere Termine gleichzeitig.'
                : 'Fügen Sie mehrere Termine gleichzeitig hinzu.'
        "
        :title="hasInitialData ? 'Regelmäßigkeit bearbeiten' : 'Regelmäßigkeit erstellen'"
    >
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <template v-if="!hasInitialData">
                    <UFormField label="Wochentyp" name="wochentyp" required>
                        <USelect
                            v-model="state.wochentyp"
                            :items="['H-Woche', 'N-Woche']"
                            class="w-full"
                            placeholder="Wochentyp auswählen"
                        />
                    </UFormField>
                    <UFormField label="Wochentag" name="wochentag" required>
                        <USelect
                            v-model="state.wochentag"
                            :items="wochentagOptions"
                            class="w-full"
                            placeholder="Wochentag auswählen"
                        />
                    </UFormField>
                    <UFormField label="Block" name="block" required>
                        <USelect
                            v-model="state.block"
                            :items="(settings.blocks ?? []) as any[]"
                            class="w-full"
                            label-key="bezeichnung"
                            placeholder="Block auswählen"
                            value-key="schemaId"
                        />
                    </UFormField>
                    <USeparator label="Zeitraum" />
                    <UAlert
                        v-if="!(state.wochentyp && state.wochentag && state.block)"
                        color="info"
                        description="Bitte wählen Sie zunächst Wochentyp, -tag und Block aus."
                        icon="i-lucide-info"
                        title="Keine Slots verfügbar!"
                        variant="subtle"
                    />
                    <UAlert
                        v-else-if="datesAvailable.length == 0"
                        color="error"
                        description="Keine verfügbaren Daten für diese Kombination an Wochentyp, -tag und
                        Block."
                        icon="i-lucide-triangle-alert"
                        title="Keine Slots verfügbar!"
                    />
                    <UFormField class="w-full" label="Start" name="start" required>
                        <OtiumDateSelector
                            v-if="!loading"
                            v-model="state.start"
                            :options="datesAvailable"
                            full-size
                            hide-today
                        />
                    </UFormField>
                    <UFormField class="w-full" label="Ende" name="end" required>
                        <OtiumDateSelector
                            v-if="!loading"
                            v-model="state.end"
                            :options="datesAvailable"
                            full-size
                            hide-today
                        />
                    </UFormField>
                    <USeparator label="Details" />
                </template>
                <UFormField label="Ort" name="location" required>
                    <UInput
                        v-model="state.location"
                        class="w-full"
                        placeholder="Ort eingeben"
                    />
                </UFormField>
                <USwitch v-model="state.hasTutor" label="Betreuer:in zuweisen" />
                <USwitch v-model="state.hasMaxTn" label="Teilnehmer:innen begrenzen" />
                <UFormField v-if="state.hasTutor" label="Betreuer:in" name="tutor" required>
                    <PersonSelector
                        v-model="state.tutor"
                        class="w-full"
                        placeholder="Betreuer:in auswählen"
                    />
                </UFormField>
                <UFormField
                    v-if="state.hasMaxTn"
                    required
                    label="Maximale Teilnehmer:innen"
                    name="maxTn"
                >
                    <UInputNumber
                        v-model="state.maxTn"
                        :min="1"
                        class="w-full"
                        color="neutral"
                        placeholder="Maximale Teilnehmer:innen-Zahl eingeben"
                    />
                </UFormField>
                <UButton
                    :icon="hasInitialData ? 'i-lucide-check' : 'i-lucide-plus'"
                    :label="hasInitialData ? 'Ändern' : 'Erstellen'"
                    class="mt-4"
                    size="lg"
                    type="submit"
                />
            </UForm>
        </template>
        <template v-if="hasInitialData" #footer>
            <span class="text-muted text-sm">
                Änderungen betreffen nur zukünftige Termine dieser Wiederholung.
            </span>
        </template>
    </UModal>
</template>

<style scoped></style>
