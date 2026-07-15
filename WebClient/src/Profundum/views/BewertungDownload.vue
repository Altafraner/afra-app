<script lang="ts" setup>
import { reactive, ref, shallowRef } from 'vue';
import { useManagement } from '@/Profundum/composables/verwaltung';
import { useFeedback } from '@/Profundum/composables/feedback';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { CalendarDate } from '@internationalized/date';
import { FormError, FormSubmitEvent } from '@nuxt/ui';
import { UserInfoMinimal } from '@/models/user/user';

interface Option<T> {
    label: string;
    value: T;
}

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Feedback',
        to: {
            name: 'Profundum-Feedback-Abgeben',
        },
    },
    {
        label: 'Drucken',
    },
];

const verwaltung = useManagement();
const feedback = useFeedback();

const url = ref<string | undefined>(undefined);
const slotsLoading = ref(true);
const schuljahrOptions = shallowRef<Option<number>[]>([]);

async function setup() {
    const slots = await verwaltung.getSlots();
    if (!slots) return;
    schuljahrOptions.value = Array.from(new Set(slots.map((x) => x.jahr))).map((jahr) => ({
        label: `${jahr} / ${jahr + 1}`,
        value: jahr,
    }));
    slotsLoading.value = false;
}

const zeitraumOptions = [
    { label: 'Halbjahr', value: true, description: 'Enthält nur Quartale 1 und 2' },
    { label: 'Endjahr', value: false, description: 'Enthält alle Quartale' },
];
const scopeOptions = [
    {
        label: 'Einzeln',
        value: false,
        description: 'Laden sie den Bogen für eine einzelne Schüler:in herunter',
    },
    { label: 'Gesammelt', value: true, description: 'Mehrere Bögen auf einmal herunterladen' },
];
const singleOptions = [
    { label: 'Eine(r)', value: true, description: 'Eine Schüler:in pro Datei' },
    { label: 'Mehrere', value: false, description: 'Mehrere Schüler:innen pro Datei' },
];
const doublesidedOptions = [
    { label: 'Einseitig', value: false, description: 'Keine Leerseiten' },
    { label: 'Doppelseitig', value: true, description: 'Enthält ggf. Leerseiten' },
];

const groupingOptions = [
    {
        label: 'Klasse',
        value: 'class',
        description: 'Gruppiert die Schüler:innen in Dateien nach ihrer Klasse',
    },
    {
        label: 'Gymnasiale Mentor:in',
        value: 'gm',
        description:
            'Gruppiert die Schüler:innen in Dateien nach ihren Gymnasialen Mentor:innen',
    },
];
setup();

type BewertungGrouping = 'gm' | 'class';
interface FormSchema {
    schuljahr: number | undefined;
    date: CalendarDate | undefined;
    isHalbjahr: boolean | undefined;
    isBatched: boolean | undefined;
    isSingleStudentBatched: boolean | undefined;
    grouping: BewertungGrouping[];
    isDoubleSided: boolean | undefined;
    person: string | undefined;
}

const state = reactive<FormSchema>({
    schuljahr: undefined,
    date: undefined,
    isHalbjahr: undefined,
    isBatched: undefined,
    isSingleStudentBatched: undefined,
    grouping: [],
    isDoubleSided: undefined,
    person: undefined,
});

function validate(state: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (!state.schuljahr) {
        errors.push({ name: 'schuljahr', message: `Bitte geben Sie das Schuljahr an.` });
    }

    if (!state.date) {
        errors.push({ name: 'date', message: `Bitte geben Sie das Ausgabedatum an.` });
    }

    if (state.isHalbjahr === undefined) {
        errors.push({ name: 'isHalbjahr', message: `Bitte wählen Sie einen Zeitraum.` });
    }

    if (state.isBatched === undefined) {
        errors.push({ name: 'isBatched', message: `Bitte wählen Sie einen Modus.` });
    }

    if (state.isBatched && state.isSingleStudentBatched === undefined) {
        errors.push({
            name: 'isSingleStudentBatched',
            message: `Bitte wählen Sie eine Option.`,
        });
    }

    if (state.isBatched && !state.isSingleStudentBatched && state.isDoubleSided === undefined) {
        errors.push({ name: 'isDoubleSided', message: `Bitte wählen Sie eine Option.` });
    }

    if (state.isBatched === false && state.person === undefined) {
        errors.push({ name: 'student', message: `Bitte wählen Sie eine Schüler:in.` });
    }

    return errors;
}

function submit(event: FormSubmitEvent<FormSchema>) {
    url.value = download();
    function download() {
        const date = event.data.date!.toDate('Etc/UTC');
        if (!event.data.isBatched) {
            return feedback.downloadForStudent(
                event.data.person!,
                event.data.schuljahr!,
                event.data.isHalbjahr!,
                date,
            );
        }
        if (event.data.isSingleStudentBatched) {
            return feedback.downloadForAll(
                event.data.schuljahr!,
                event.data.isHalbjahr!,
                true,
                false,
                false,
                date,
                false,
            );
        }
        return feedback.downloadForAll(
            event.data.schuljahr!,
            event.data.isHalbjahr!,
            false,
            event.data.grouping.includes('gm'),
            event.data.grouping.includes('class'),
            date,
            event.data.isDoubleSided!,
        );
    }
}
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <h1>Feedback-Bogen herunterladen</h1>

    <UTheme
        :props="{
            formField: { orientation: 'horizontal' },
            radioGroup: { orientation: 'horizontal', variant: 'table' },
        }"
        :ui="{
            formField: {
                wrapper: 'flex-1 shrink',
                container: 'flex-2',
            },
            radioGroup: {
                item: 'flex-1',
                label: 'my-0',
                description: 'my-0',
            },
            checkbox: {
                label: 'my-0',
                description: 'my-0',
            },
        }"
    >
        <UForm :state="state" :validate="validate" class="flex flex-col gap-4" @submit="submit">
            <UFormField label="Schuljahr" name="schuljahr" required>
                <USelect
                    v-model="state.schuljahr"
                    :items="schuljahrOptions"
                    :loading="slotsLoading"
                    class="w-full"
                    placeholder="Schuljahr wählen"
                    size="lg"
                />
            </UFormField>
            <UFormField label="Datum der Ausgabe" name="date" required>
                <UInputDate v-model="state.date as any" class="w-full" size="lg" />
            </UFormField>
            <UFormField
                :ui="{
                    root: 'place-items-start',
                    wrapper: 'mt-3 flex-1',
                }"
                label="Modus"
                name="isHalbjahr"
                required
            >
                <URadioGroup v-model="state.isHalbjahr" :items="zeitraumOptions" />
            </UFormField>
            <UFormField
                :ui="{
                    root: 'place-items-start',
                    wrapper: 'mt-3 flex-1',
                }"
                label="Modus"
                name="isBatched"
                required
            >
                <URadioGroup v-model="state.isBatched" :items="scopeOptions" />
            </UFormField>
            <UFormField
                v-if="state.isBatched"
                :ui="{
                    root: 'place-items-start',
                    wrapper: 'mt-3 flex-1',
                }"
                label="Schüler:innen pro Datei"
                name="isSingleStudentBatched"
                required
            >
                <URadioGroup v-model="state.isSingleStudentBatched" :items="singleOptions" />
            </UFormField>
            <UFormField
                v-if="state.isBatched && state.isSingleStudentBatched === false"
                help="Wählen Sie keine Option aus, enthalten sie eine Datei mit allen Schüler:innen. Wählen Sie mehrere aus, wird das kartesische Produkt gebildet."
                hint="Optional"
                label="Gruppierung"
                name="grouping"
            >
                <UCheckboxGroup
                    v-model="state.grouping"
                    :items="groupingOptions"
                    variant="card"
                />
            </UFormField>
            <UFormField
                v-if="state.isBatched && state.isSingleStudentBatched === false"
                :ui="{
                    root: 'place-items-start',
                    wrapper: 'mt-3 flex-1',
                }"
                help="Beim Doppelseitigen Drucken sind ggf. Leerseiten nötig, damit nicht inhalte verschiedener Schüler:innen auf das selbe Blatt gedruckt werden. Hat keinen Einfluss auf die dargestellte Datenmenge"
                label="Druckoption"
                name="isDoubleSided"
                required
            >
                <URadioGroup v-model="state.isDoubleSided" :items="doublesidedOptions" />
            </UFormField>
            <UFormField
                v-if="state.isBatched === false"
                label="Schüler:in"
                name="student"
                required
            >
                <PersonSelectorNuxt
                    v-model="state.person"
                    :filter="
                        (student: UserInfoMinimal) =>
                            student.rolle == 'Mittelstufe' || student.rolle == 'Oberstufe'
                    "
                    class="w-full"
                    placeholder="Schüler:in wählen"
                    size="lg"
                />
            </UFormField>
            <UButton
                icon="i-lucide-arrow-down-to-line"
                label="Herunterladen"
                size="lg"
                type="submit"
            />
        </UForm>
    </UTheme>
</template>

<style scoped></style>
