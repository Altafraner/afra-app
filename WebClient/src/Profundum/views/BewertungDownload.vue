<script lang="ts" setup>
import Form from '@primevue/forms/form';
import { Button, DatePicker, Message, Select, SelectButton } from 'primevue';
import AfraPersonSelector from '@/Otium/components/Form/AfraPersonSelector.vue';
import { ref, shallowRef } from 'vue';
import type { FormResolverOptions, FormSubmitEvent } from '@primevue/forms';
import { useManagement } from '@/Profundum/composables/verwaltung';
import { useFeedback } from '@/Profundum/composables/feedback';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

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
        route: {
            name: 'Profundum-Feedback-Abgeben',
        },
    },
    {
        label: 'Drucken',
        route: {
            name: 'Profundum-Feedback-Download',
        },
    },
];

const verwaltung = useManagement();
const feedback = useFeedback();

const url = ref('');
const slotsLoading = ref(true);
const schuljahrOptions = shallowRef<Option<number>[]>([]);

async function setup() {
    const slots = await verwaltung.getSlots();
    schuljahrOptions.value = Array.from(new Set(slots.map((x) => x.jahr))).map((jahr) => ({
        label: `${jahr} / ${jahr + 1}`,
        value: jahr,
    }));
    slotsLoading.value = false;
}

const zeitraumOptions = [
    { label: 'Halbjahr', value: true },
    { label: 'Endjahr', value: false },
];
const scopeOptions = [
    { label: 'Einzeln', value: false },
    { label: 'Gesammelt', value: true },
];
const singleOptions = [
    { label: 'Eine(r)', value: true },
    { label: 'Mehrere', value: false },
];
const doublesidedOptions = [
    { label: 'Einseitig', value: false },
    { label: 'Doppelseitig', value: true },
];
const yesNoOption = [
    { label: 'Ja', value: true },
    { label: 'Nein', value: false },
];

const resolver = (e: FormResolverOptions): Record<string, any> => {
    const errors: Record<string, string[]> = {
        schuljahr: [],
        zeitraum: [],
        ausgabe: [],
        batch: [],
        person: [],
        single: [],
        byGm: [],
        byClass: [],
        doublesided: [],
    };

    if (!e.values['schuljahr']) {
        errors['schuljahr'].push(`Schuljahr ist erforderlich!`);
    }

    if (e.values['zeitraum'] == null) {
        errors['zeitraum'].push(`Zeitraum ist erforderlich!`);
    }

    if (e.values['ausgabe'] == null) {
        errors['ausgabe'].push(`Ausgabedatum ist erforderlich!`);
    }

    if (e.values['batch'] == null) {
        errors['batch'].push('Bitte treffen Sie eine Auswahl!');
    }

    if (!e.values['batch'] && e.values['person'] == null) {
        errors['person'].push('Bitte wählen Sie eine Schüler:in!');
    }

    if (e.values['batch'] && e.values['single'] == null) {
        errors['single'].push('Bitte treffen Sie eine Auswahl!');
    }

    if (e.values['batch'] && !e.values['single'] && e.values['doublesided'] == null) {
        errors['doublesided'].push('Bitte treffen Sie eine Auswahl!');
    }

    if (e.values['batch'] && !e.values['single'] && e.values['byGm'] == null) {
        errors['byGm'].push('Bitte treffen Sie eine Auswahl!');
    }

    if (e.values['batch'] && !e.values['single'] && e.values['byClass'] == null) {
        errors['byClass'].push('Bitte treffen Sie eine Auswahl!');
    }

    return { values: e.values, errors };
};

function download(values: Record<string, any>) {
    if (!values['batch']) {
        return feedback.downloadForStudent(
            values['person'],
            values['schuljahr'],
            values['zeitraum'],
            values['ausgabe'],
        );
    }
    if (values['single']) {
        return feedback.downloadForAll(
            values['schuljahr'],
            values['zeitraum'],
            true,
            false,
            false,
            values['ausgabe'],
            false,
        );
    }
    return feedback.downloadForAll(
        values['schuljahr'],
        values['zeitraum'],
        false,
        values['groupGm'],
        values['groupClass'],
        values['ausgabe'],
        values['doublesided'],
    );
}

const submit = ({ valid, values }: FormSubmitEvent) => {
    url.value = '';
    if (!valid) return;
    url.value = download(values);
};

setup();
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <h1>Feedback-Bogen herunterladen</h1>
    <Form v-slot="$form" :resolver="resolver" @submit="submit">
        <div class="flex flex-col w-full gap-4">
            <div class="grid grid-cols-2 justify-between items-center gap-4">
                <label for="schuljahr">Schuljahr</label>
                <div class="w-full">
                    <Select
                        id="schuljahr"
                        :loading="slotsLoading"
                        :options="schuljahrOptions"
                        fluid
                        name="schuljahr"
                        optionLabel="label"
                        optionValue="value"
                        placeholder="Auswählen"
                    />
                    <Message
                        v-if="$form.schuljahr?.invalid"
                        severity="error"
                        size="small"
                        variant="simple"
                    >
                        {{ $form.schuljahr.error }}
                    </Message>
                </div>
                <label for="ausgabe">Datum der Ausgabe</label>
                <div class="w-full">
                    <DatePicker id="ausgabe" fluid name="ausgabe" placeholder="Auswählen" />
                    <Message
                        v-if="$form.ausgabe?.invalid"
                        severity="error"
                        size="small"
                        variant="simple"
                    >
                        {{ $form.ausgabe.error }}
                    </Message>
                </div>
                <label for="zeitraum">Zeitraum</label>
                <div class="w-full">
                    <SelectButton
                        id="zeitraum"
                        :options="zeitraumOptions"
                        fluid
                        name="zeitraum"
                        optionLabel="label"
                        optionValue="value"
                    />
                    <Message
                        v-if="$form.zeitraum?.invalid"
                        severity="error"
                        size="small"
                        variant="simple"
                    >
                        {{ $form.zeitraum.error }}
                    </Message>
                </div>
                <label for="batch">Modus</label>
                <div class="w-full">
                    <SelectButton
                        id="batch"
                        :options="scopeOptions"
                        fluid
                        name="batch"
                        optionLabel="label"
                        optionValue="value"
                    />
                    <Message
                        v-if="$form.batch?.invalid"
                        severity="error"
                        size="small"
                        variant="simple"
                    >
                        {{ $form.batch.error }}
                    </Message>
                </div>
                <template v-if="$form.batch?.value == true">
                    <label class="font-semibold" for="single">Schüler:innen pro Datei</label>
                    <div class="w-full">
                        <SelectButton
                            id="single"
                            :options="singleOptions"
                            fluid
                            name="single"
                            optionLabel="label"
                            optionValue="value"
                        />
                        <Message
                            v-if="$form.single?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.single.error }}
                        </Message>
                    </div>
                </template>
                <template v-if="$form.batch?.value == true && $form.single?.value === false">
                    <label class="font-semibold" for="groupClass">Gruppieren nach Klasse</label>
                    <div class="w-full">
                        <SelectButton
                            id="groupClass"
                            :options="yesNoOption"
                            fluid
                            name="groupClass"
                            optionLabel="label"
                            optionValue="value"
                        />
                        <Message
                            v-if="$form.groupClass?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.groupClass.error }}
                        </Message>
                    </div>
                    <label class="font-semibold" for="groupGm"
                        >Gruppieren nach gymnasiale(r) Mentor(in)</label
                    >
                    <div class="w-full">
                        <SelectButton
                            id="groupGm"
                            :options="yesNoOption"
                            fluid
                            name="groupGm"
                            optionLabel="label"
                            optionValue="value"
                        />
                        <Message
                            v-if="$form.groupGm?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.groupGm.error }}
                        </Message>
                    </div>
                    <label class="font-semibold" for="groupGm">Druck</label>
                    <div class="w-full">
                        <SelectButton
                            id="doublesided"
                            :options="doublesidedOptions"
                            fluid
                            name="doublesided"
                            optionLabel="label"
                            optionValue="value"
                        />
                        <Message
                            v-if="$form.doublesided?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.doublesided.error }}
                        </Message>
                    </div>
                </template>
                <template v-if="$form.batch?.value === false">
                    <span>Schüler:in</span>
                    <div>
                        <AfraPersonSelector
                            v-if="$form.batch?.value === false"
                            id="person"
                            name="person"
                        >
                            <template #label>Schüler:in wählen</template>
                        </AfraPersonSelector>
                        <Message
                            v-if="$form.person?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.person.error }}
                        </Message>
                    </div>
                </template>
            </div>

            <Button label="Herunterladen" type="submit" />

            <Message v-if="url !== '' && url !== undefined" severity="success">
                Der Download sollte in wenigen Momenten starten. Bei großen Downloads kann es
                bis zu 15 Sekunden dauern, bis die Dateien generiert sind. Falls der Download
                nicht starten sollte, folgen Sie diesem Link:
                <a :href="url" class="underline" download target="_blank"
                    >Download manuell starten.</a
                >
            </Message>
        </div>
    </Form>
</template>

<style scoped></style>
