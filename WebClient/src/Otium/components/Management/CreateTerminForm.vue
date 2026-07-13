<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { formatTutor } from '@/helpers/formatters';
import OtiumDateSelector from '@/Otium/components/Form/OtiumDateSelector.vue';
import { UserInfoMinimal } from '@/models/user/user.ts';
import { FormError, FormSubmitEvent } from '@nuxt/ui';

const emit = defineEmits<{
    close: [
        {
            date: string;
            block: string;
            ort: string;
            person: string | undefined;
            maxEnrollments: number | undefined;
            overrideBezeichnung: string | undefined;
            overrideBeschreibung: string | undefined;
        },
    ];
}>();

const settings = useOtiumStore();

const dates = ref<any[]>([]);
const datumSelected = ref<any | null>(null);
const loading = ref(true);
const personen = ref<{ id: string; name: string }[]>([]);

function validate(state: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (!state.slotId)
        errors.push({ name: 'slotId', message: 'Es muss ein Block gesetzt sein' });
    if (!state.location || state.location.length < 1)
        errors.push({ name: 'location', message: 'Es muss ein Ort gesetzt sein' });
    if (state.location && state.location.length > 20)
        errors.push({ name: 'location', message: 'Der Ort darf maximal 20 Zeichen lang sein' });
    if (state.hasTutor && !state.person)
        errors.push({ name: 'tutor', message: 'Es muss eine Person ausgewählt sein' });
    if (state.hasMaxTn && !state.maxTn)
        errors.push({
            name: 'maxTn',
            message: 'Bitte wählen Sie eine maximal Teilnehmer:innen-Zahl',
        });
    if (state.hasOverwriteLabel && (!state.label || state.label.length < 1))
        errors.push({ name: 'label', message: 'Bitte geben Sie eine Bezeichnung ein' });
    if (state.hasOverwriteLabel && state.label && state.label.length > 70)
        errors.push({ name: 'label', message: 'Bitte geben Sie maximal 70 Zeichen ein' });
    if (state.hasOverwriteDescription && (!state.description || state.description.length < 1))
        errors.push({ name: 'description', message: 'Bitte geben Sie eine Beschreibung ein' });
    if (state.hasOverwriteDescription && state.description && state.description.length > 500)
        errors.push({
            name: 'description',
            message: 'Bitte geben Sie maximal 500 Zeichen ein.',
        });

    return errors;
}

async function getTermine() {
    await settings.updateSchuljahr();
    dates.value = settings.schuljahr ?? [];
    datumSelected.value = (settings.defaultDay as any | null)?.datum;
}

async function getPersonen() {
    const personenMapper = (person: UserInfoMinimal) => {
        return {
            id: person.id,
            name: `${formatTutor(person)} (${person.rolle})`,
        };
    };

    await settings.updatePersonen();
    personen.value = (settings.personen as UserInfoMinimal[] | null)?.map(personenMapper) ?? [];
}

async function setup() {
    loading.value = true;
    const personPromise = getPersonen();
    const terminePromise = getTermine();

    await Promise.all([personPromise, terminePromise]);
    loading.value = false;
}

function submit(event: FormSubmitEvent<FormSchema>) {
    emit('close', {
        date: event.data.date!,
        block: event.data.slotId!,
        ort: event.data.location!,
        person: event.data.person,
        maxEnrollments: event.data.maxTn,
        overrideBezeichnung: event.data.label,
        overrideBeschreibung: event.data.description,
    });
}

setup();

interface FormSchema {
    date: string | undefined;
    slotId: string | undefined;
    location: string | undefined;
    hasTutor: boolean;
    hasMaxTn: boolean;
    hasOverwriteLabel: boolean;
    hasOverwriteDescription: boolean;
    person: string | undefined;
    maxTn: number | undefined;
    label: string | undefined;
    description: string | undefined;
}

const state = reactive<FormSchema>({
    date: (settings.defaultDay as any | null)?.datum,
    description: undefined,
    hasMaxTn: false,
    hasOverwriteDescription: false,
    hasOverwriteLabel: false,
    hasTutor: false,
    label: undefined,
    location: undefined,
    maxTn: undefined,
    person: undefined,
    slotId: undefined,
});

const blocksAvailable = computed(() => {
    return (
        ((settings.schuljahr ?? []).find((s: any) => s.datum === state.date) as any | null)
            ?.blocks ?? []
    );
});

watch(blocksAvailable, (newValue) => {
    if (newValue.some((b: any) => b.schemaId == state.slotId)) return;
    state.slotId = undefined;
});

watch(
    () => state.hasTutor,
    () => {
        if (!state.hasTutor) {
            state.person = undefined;
        }
    },
);
watch(
    () => state.hasMaxTn,
    () => {
        if (!state.hasMaxTn) {
            state.maxTn = undefined;
        }
    },
);
watch(
    () => state.hasOverwriteLabel,
    () => {
        if (!state.hasOverwriteLabel) {
            state.label = undefined;
        }
    },
);
watch(
    () => state.hasOverwriteDescription,
    () => {
        if (!state.hasOverwriteDescription) {
            state.description = undefined;
        }
    },
);
</script>

<template>
    <UModal title="Termin erstellen">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Datum" name="date" required>
                    <OtiumDateSelector
                        v-if="!loading"
                        v-model="state.date"
                        :options="dates"
                        full-size
                        hide-today
                    />
                </UFormField>
                <UFormField label="Block" name="slotId" required>
                    <USelect
                        v-model="state.slotId"
                        :items="blocksAvailable"
                        class="w-full"
                        label-key="bezeichnung"
                        placeholder="Block wählen"
                        value-key="schemaId"
                    >
                        <template
                            v-if="!blocksAvailable || blocksAvailable.length === 0"
                            #content-top
                        >
                            <span class="p-2 text-sm text-dimmed"
                                ><UIcon
                                    class="h-4 inline-block -translate-y-0.5"
                                    name="i-lucide-triangle-alert"
                                />
                                Keine Blöcke verfügbar
                            </span>
                        </template>
                    </USelect>
                </UFormField>
                <UFormField label="Ort" name="location" required>
                    <UInput
                        v-model="state.location"
                        class="w-full"
                        placeholder="Ort eingeben"
                    />
                </UFormField>
                <USeparator label="Details" />
                <USwitch
                    v-model="state.hasTutor"
                    label="Betreuer:in zuweisen"
                    name="hasTutor"
                />
                <USwitch
                    v-model="state.hasMaxTn"
                    label="Max. Teilnehmer:innen beschränken"
                    name="hasMaxTn"
                />
                <USwitch
                    v-model="state.hasOverwriteLabel"
                    label="Abweichende Bezeichnung vergeben"
                    name="hasOverwriteLabel"
                />
                <USwitch
                    v-model="state.hasOverwriteDescription"
                    label="Abweichende Beschreibung vergeben"
                    name="hasOverwriteDescription"
                />
                <UFormField v-if="state.hasTutor" label="Betreuer:in" name="tutor">
                    <PersonSelectorNuxt
                        v-model="state.person"
                        class="w-full"
                        placeholder="Betreuer:in wählen"
                    />
                </UFormField>
                <UFormField
                    v-if="state.hasMaxTn"
                    label="Maximale Teilnehmer:innen"
                    name="maxTn"
                >
                    <UInputNumber
                        v-model="state.maxTn"
                        :min="0"
                        class="w-full"
                        color="neutral"
                        placeholder="Anzahl maximaler Teilnehmer:innen eingeben"
                    />
                </UFormField>
                <UFormField v-if="state.hasOverwriteLabel" label="Bezeichnung" name="label">
                    <UInput
                        v-model="state.label"
                        class="w-full"
                        placeholder="Abweichende Bezeichnung eingeben"
                    />
                </UFormField>
                <UFormField
                    v-if="state.hasOverwriteDescription"
                    label="Beschreibung"
                    name="description"
                >
                    <UTextarea
                        v-model="state.description"
                        :rows="3"
                        autoresize
                        class="w-full"
                        placeholder="Abweichende Beschreibung eingeben"
                    />
                </UFormField>
                <UButton icon="i-lucide-plus" label="Erstellen" type="submit" />
            </UForm>

            <!--Form
                v-if="!loading"
                v-slot="$form"
                :resolver="resolve"
                class="flex flex-col gap-3"
                @submit="submit"
            >
                <div class="font-bold">Zeitpunkt</div>
                <OtiumDateSelector
                    v-if="!loading"
                    v-model="datumSelected"
                    :options="dates"
                    hide-today
                    name="date"
                />
                <Select
                    v-model="blockSelected"
                    :options="dateSelected?.blocks ?? []"
                    name="block"
                >
                    <template #value="{ value }">
                        {{ value?.bezeichnung ?? 'Keine Blöcke verfügbar' }}
                    </template>
                    <template #option="{ option }">
                        {{ option?.bezeichnung }}
                    </template>
                </Select>
                <div class="font-bold mt-4">Details</div>
                <div class="w-full">
                    <FloatLabel class="w-full" variant="on">
                        <InputText id="ort" v-model="ortSelected" fluid name="ort" />
                        <label for="ort">Ort</label>
                    </FloatLabel>
                    <Message
                        v-if="$form.ort?.invalid"
                        severity="error"
                        size="small"
                        variant="simple"
                    >
                        {{ $form.ort.error.message }}
                    </Message>
                </div>
                <div class="flex justify-between mt-4">
                    <label for="betreuerSwitch">Betreuer:in zuweisen</label>
                    <ToggleSwitch v-model="betreuerZuweisenSelected" if="betreuerSwitch" />
                </div>
                <PersonSelector
                    id="betreuerSelect"
                    v-model="personSelected"
                    :disabled="!betreuerZuweisenSelected"
                    name="tutor"
                    required
                />
                <div class="flex justify-between mt-4">
                    <label for="maxEnrollmentSwitch">Teilnehmer:innen-Zahl beschränken</label>
                    <ToggleSwitch
                        v-model="maxEnrollmentsSetzenSelected"
                        if="maxEnrollmentSwitch"
                    />
                </div>
                <FloatLabel class="w-full" variant="on">
                    <InputNumber
                        id="maxEnrollmentInput"
                        v-model="maxEnrollmentsSelected"
                        :disabled="!maxEnrollmentsSetzenSelected"
                        fluid
                        name="maxEnrollments"
                    />
                    <label for="maxEnrollmentInput">max. Teilnehmer:innen</label>
                </FloatLabel>
                <div class="flex justify-between mt-4">
                    <label for="bezeichnungSwitch">Bezeichnung überschreiben</label>
                    <ToggleSwitch v-model="bezeichnungSetzenSelected" if="bezeichnungSwitch" />
                </div>
                <FloatLabel class="w-full" variant="on">
                    <InputText
                        id="bezeichnungInput"
                        v-model="bezeichnungSelected"
                        :disabled="!bezeichnungSetzenSelected"
                        fluid
                        maxlength="70"
                        name="bezeichnung"
                    />
                    <label for="bezeichnungInput">Bezeichnung</label>
                </FloatLabel>
                <Button class="mt-4" label="Erstellen" severity="primary" type="submit" />
            </Form-->
        </template>
    </UModal>
</template>

<style scoped></style>
