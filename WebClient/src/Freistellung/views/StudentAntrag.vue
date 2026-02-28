<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { CalendarDateTime } from '@internationalized/date';
import type { FormError, FormSubmitEvent } from '@nuxt/ui';
import { mande, type MandeError } from 'mande';
import { useRouter } from 'vue-router';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import ADateTimePicker from '@/components/Form/ADateTimePicker.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import type { CreateFreistellungsantrag } from '@/Freistellung/models/freistellung';

const router = useRouter();
const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } },
    { label: 'Neuer Antrag', route: { name: 'Freistellung-Neu' } },
];

await store.updateLehrer();

const lehrerOptions = computed(
    () =>
        store.lehrer?.map((l) => ({ label: `${l.nachname}, ${l.vorname}`, value: l.id })) ?? [],
);

interface StundeRow {
    datum: string;
    block: number;
    fach: string;
    lehrerId: string | undefined;
}

interface FormSchema {
    grund: string;
    von: CalendarDateTime | undefined;
    bis: CalendarDateTime | undefined;
    beschreibung: string;
}

const state = reactive<FormSchema>({
    grund: '',
    von: undefined,
    bis: undefined,
    beschreibung: '',
});
const stunden = ref<StundeRow[]>([]);
const loading = ref(false);

function toDateStr(date: CalendarDateTime): string {
    return date.toString().split('T')[0];
}

function formatDateJs(dateStr: string): string {
    const [y, m, d] = dateStr.split('-');
    return `${d}.${m}.${y}`;
}

const tage = computed(() => {
    if (!state.von) return [];
    const bis = state.bis ?? state.von;
    const days: string[] = [];
    let cur = state.von.set({ hour: 0, minute: 0, second: 0 });
    const end = bis.set({ hour: 0, minute: 0, second: 0 });
    while (cur.compare(end) <= 0) {
        days.push(toDateStr(cur));
        cur = cur.add({ days: 1 });
    }
    return days;
});

const tagOptions = computed(() =>
    tage.value.map((d) => ({ label: formatDateJs(d), value: d })),
);

watch(tage, (newTage) => {
    const validDates = new Set(newTage);
    stunden.value = stunden.value.filter((s) => validDates.has(s.datum));
});

const stundenValid = computed(
    () =>
        stunden.value.length > 0 &&
        stunden.value.every((s) => s.datum && s.block > 0 && s.fach.trim() && s.lehrerId),
);

function addStunde() {
    stunden.value.push({
        datum: tagOptions.value[0]?.value ?? '',
        block: 1,
        fach: '',
        lehrerId: undefined,
    });
}

function removeStunde(index: number) {
    stunden.value.splice(index, 1);
}

function validate(formState: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];
    if (!formState.von) errors.push({ name: 'von', message: 'Bitte einen Beginn angeben.' });
    if (!formState.bis) errors.push({ name: 'bis', message: 'Bitte ein Ende angeben.' });
    if (formState.von && formState.bis && formState.bis.compare(formState.von) < 0)
        errors.push({ name: 'bis', message: 'Das Ende darf nicht vor dem Beginn liegen.' });
    if (!formState.grund?.trim())
        errors.push({ name: 'grund', message: 'Bitte einen Kurztitel angeben.' });
    if (!formState.beschreibung?.trim())
        errors.push({ name: 'beschreibung', message: 'Bitte den Grund angeben.' });
    return errors;
}

async function submit(event: FormSubmitEvent<FormSchema>) {
    if (!stundenValid.value) {
        toast.add({
            color: 'warning',
            title: 'Fehlende Angaben',
            description: 'Bitte mindestens eine betroffene Stunde vollständig angeben.',
        });
        return;
    }

    loading.value = true;
    const payload: CreateFreistellungsantrag = {
        grund: event.data.grund.trim(),
        beschreibung: event.data.beschreibung.trim(),
        von: event.data.von!.toString(),
        bis: event.data.bis!.toString(),
        stunden: stunden.value.map((s) => ({
            datum: s.datum,
            block: s.block,
            fach: s.fach.trim(),
            lehrerId: s.lehrerId!,
        })),
    };

    const api = mande('/api/freistellung/sus');
    try {
        await api.post(payload);
        store.meineAntraege = null;
        toast.add({
            color: 'success',
            title: 'Antrag gestellt',
            description: 'Dein Freistellungsantrag wurde erfolgreich eingereicht.',
        });
        await router.push({ name: 'Freistellung-Meine' });
    } catch (e) {
        const mandeError = e as MandeError<{ error?: string }>;
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: mandeError.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.',
        });
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsantrag stellen</h1>
    <p>
        Hier kannst du einen Freistellungsantrag (Antrag auf Unterrichtsbefreiung) einreichen.
        Wähle den Zeitraum und gib für jede betroffene Unterrichtsstunde das Datum, den Block,
        das Fach und die Lehrkraft an.
    </p>

    <UForm
        :state="state"
        :validate="validate"
        class="flex flex-col gap-6 mt-4"
        style="max-width: 50rem"
        @submit="submit"
    >
        <UFormField label="Kurztitel" name="grund" required>
            <UInput v-model="state.grund" :maxlength="200" class="w-full" />
        </UFormField>

        <UFormField label="Beginn (Datum und Uhrzeit)" name="von" required>
            <ADateTimePicker v-model="state.von as CalendarDateTime | undefined" />
        </UFormField>

        <UFormField label="Ende (Datum und Uhrzeit)" name="bis" required>
            <ADateTimePicker v-model="state.bis as CalendarDateTime | undefined" />
        </UFormField>

        <UFormField label="Grund der Freistellung" name="beschreibung" required>
            <UTextarea
                v-model="state.beschreibung"
                :rows="4"
                :maxlength="1000"
                class="w-full"
                placeholder="Bitte beschreibe den Grund deines Freistellungsantrags..."
            />
        </UFormField>

        <div class="flex flex-col gap-2">
            <label class="font-semibold">Betroffene Unterrichtsstunden</label>
            <p v-if="!tagOptions.length" class="text-sm text-muted">
                Bitte zuerst den Zeitraum auswählen.
            </p>

            <template v-else>
                <div v-if="stunden.length > 0" class="overflow-x-auto">
                    <table class="w-full text-sm border-collapse">
                        <thead>
                            <tr class="text-left border-b border-default">
                                <th class="py-1 pr-3">Datum</th>
                                <th class="py-1 pr-3">Block</th>
                                <th class="py-1 pr-3">Fach</th>
                                <th class="py-1 pr-3">Lehrkraft</th>
                                <th class="py-1"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr
                                v-for="(stunde, index) in stunden"
                                :key="index"
                                class="border-b border-default last:border-0"
                            >
                                <td class="py-2 pr-3" style="min-width: 9rem">
                                    <USelect
                                        v-model="stunde.datum"
                                        :items="tagOptions"
                                        label-key="label"
                                        value-key="value"
                                        class="w-full"
                                    />
                                </td>
                                <td class="py-2 pr-3" style="min-width: 8rem">
                                    <UInputNumber
                                        v-model="stunde.block"
                                        :min="1"
                                        :max="12"
                                        orientation="horizontal"
                                        class="w-full"
                                    />
                                </td>
                                <td class="py-2 pr-3" style="min-width: 10rem">
                                    <UInput
                                        v-model="stunde.fach"
                                        :maxlength="200"
                                        class="w-full"
                                        placeholder="Fach"
                                    />
                                </td>
                                <td class="py-2 pr-3" style="min-width: 12rem">
                                    <USelectMenu
                                        v-model="stunde.lehrerId"
                                        :items="lehrerOptions"
                                        label-key="label"
                                        value-key="value"
                                        class="w-full"
                                        placeholder="Lehrkraft…"
                                    />
                                </td>
                                <td class="py-2">
                                    <UButton
                                        icon="i-lucide-trash"
                                        color="error"
                                        variant="ghost"
                                        size="sm"
                                        aria-label="Stunde entfernen"
                                        @click="removeStunde(index)"
                                    />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <p v-else class="text-sm text-muted">Noch keine Stunden hinzugefügt.</p>

                <div>
                    <UButton
                        label="Stunde hinzufügen"
                        icon="i-lucide-plus"
                        color="neutral"
                        size="sm"
                        @click="addStunde"
                    />
                </div>
            </template>
        </div>

        <UButton
            label="Antrag einreichen"
            icon="i-lucide-send"
            type="submit"
            :loading="loading"
        />
    </UForm>
</template>
