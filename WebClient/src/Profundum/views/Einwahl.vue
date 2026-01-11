<script setup>
import { computed, ref } from 'vue';
import EinwahlSelectorGroup from '@/Profundum/components/EinwahlSelectorGroup.vue';
import EinwahlSelector from '@/Profundum/components/EinwahlSelector.vue';
import { Button, Select, useToast, Toast } from 'primevue';
import { mande } from 'mande';
import { useRouter } from 'vue-router';

const toast = useToast();
const router = useRouter();

const options = ref([]);
const results = ref({});

async function get() {
    const api = mande('/api/profundum/sus/wuensche');
    const profunda = await api.get();
    options.value = profunda;

    for (const option of options.value.filter((x) => x.fixed === null)) {
        results.value[option.id] = [null, null, null];
    }
}

async function send() {
    console.log('Sending, ...', results);
    const api = mande('/api/profundum/sus/wuensche');

    try {
        await api.post(results.value);
    } catch (e) {
        const errors = Array.isArray(e.body?.error)
            ? e.body.error
            : String(e.body?.error ?? 'Unbekannter Fehler')
                  .split('\n')
                  .filter(Boolean);

        toast.add({
            group: 'bc',
            severity: 'error',
            summary: 'Fehler',
            detail: 'Deine Belegwünsche sind fehlerhaft.',
            data: { errors },
            life: 8000,
        });
        return;
    }

    toast.add({
        severity: 'success',
        summary: 'Wünsche erfolgreich abgegeben',
        detail: 'Deine Wünsche wurden erfolgreich gespeichert.',
        life: 3000,
    });
}

const preSelectedRaw = computed(() => {
    const claims = {};

    for (const option of options.value.filter((x) => x.fixed === null)) {
        for (let i = 0; i < results.value[option.id].length; i++) {
            const value = results.value[option.id][i];
            const valueObj = option.options.find((opt) => opt.value === value);

            if (valueObj?.alsoIncludes?.length) {
                for (const forcedId of valueObj.alsoIncludes) {
                    if (!claims[forcedId]) claims[forcedId] = [[], [], []];
                    claims[forcedId][i] ??= [];
                    claims[forcedId][i].push(valueObj);
                }
            }
        }
    }

    return claims;
});

const preSelected = computed(() => {
    const clean = {};
    const conflicts = {};

    for (const [optId, slots] of Object.entries(preSelectedRaw.value)) {
        for (const i in slots) {
            if (!slots[i]?.length) continue;

            const unique = [...new Map(slots[i].map((v) => [v.value, v])).values()];

            if (unique.length === 1) {
                clean[optId] ??= [];
                clean[optId][i] = unique[0];
            } else {
                conflicts[optId] ??= [];
                conflicts[optId][i] = unique;
            }
        }
    }

    return { clean, conflicts };
});

const maySend = computed(() => {
    for (const option of options.value.filter((x) => x.fixed === null)) {
        if (
            option.fixed === null &&
            !results.value[option.id].every((value) => value !== null)
        ) {
            return false;
        }
    }
    return true;
});

async function startup() {
    get();
}

startup();
</script>

<template>
    <h1>Profundums-Einwahl</h1>

    <p>
        Bitte lest euch die Folgenden Hinweise aufmerksam durch. Beachtet vor allem die
        Informationen, die ihr zum Profundum erhalten habt.
        <strong> Der Zeitpunkt der Abgabe </strong> innerhalb des Einwahlfensters ist
        <strong> kein Kriterium </strong> zur Belegung.
    </p>

    <h2>Vorauswahl</h2>

    <p>Im Folgenden sind eure bereits gewählten Profunda ausgegraut aufgeführt. Bitte prüft:</p>
    <ol class="list-decimal pl-6">
        <li>Ob eure vergangenen Belegungen korrekt sind.</li>
        <li>
            Ob ihr für die Zukunft gewählte Profunda (insbesondere Fremdsprachen) weiter belegen
            wollt.
        </li>
    </ol>
    <p>
        Ist eine Belegung falsch, oder ihr möchtet eine Sprache abwählen, gebt ihr bitte
        <strong> noch keine Wünsche </strong> ab und wendet euch an
        <a href="mailto:@afra.lernsax.de">@afra.lernsax.de</a>
    </p>

    <h2>Kriterien</h2>

    <p>Bitte beachtet das Profundarium für ausführliche Informationen zu jedem Angebot.</p>

    <p>
        Um eure Wünsche abgeben zu können, wählt ihr für jeden Slot eure drei Favoriten in
        absteigender Reihenfolge. Profunda, die sich über mehrere Quartale erstrecken, werden
        automatisch dort eingetragen. Sie sind nur in ihrem ersten Quartal in der Liste zu
        finden.
    </p>

    <p>
        Weitere Einschränkungen werden euch möglicherweise in einem Fenster angezeigt, nachdem
        ihr auf "abgeben" geklickt habt. In diesem Fall nehmt ihr bitte entsprechende Änderungen
        vor und versucht es erneut. Diese Kriterien sind vom Konzept-Team vorgegeben. Wurden
        eure Wünsche übernommen, so seht ihr das an einer Bestätigung in einem grünen Fenster.
    </p>

    <h2>Matching</h2>

    <p>
        Nach dem Zeitfenster zur Einwahl berechnen wir aus den abgegebenen Wünschen eine
        Belegung, die oben genannte Kriterien erfüllt. Wir bevorzugen Erstwünsche vor
        Zweitwünschen und diese vor Drittwünschen nach
        <a
            class="text-blue-500 hover:underline cursor-pointer"
            href="https://github.com/Altafraner/afra-app"
            target="_blank"
            >veröffentlichter Berechnungsvorschrift</a
        >. Jeder soll immer einen seiner drei Wünsche erhalten, werden keine weiteren Absprachen
        getroffen.
    </p>

    <hr class="my-3" />

    <div v-for="option in options" :key="option.id" class="mb-4">
        <h2>{{ option.label }}</h2>
        <EinwahlSelectorGroup
            v-if="!option.fixed"
            v-model="results[option.id]"
            :options="option.options"
            :preSelected="preSelected.clean[option.id]"
            :conflicts="preSelected.conflicts[option.id]"
        />
        <template v-else>
            <div class="w-full">
                <Select
                    fluid
                    :disabled="true"
                    v-model="option.fixed"
                    :options="[option.fixed]"
                    option-label="label"
                />
            </div>
        </template>
    </div>
    <Button
        :disabled="!maySend"
        class="mb-4"
        fluid
        label="Überprüfen und abgeben"
        @click="send"
    />

    <Toast group="bc">
        <template #message="slotProps">
            <div class="flex flex-col gap-2">
                <div class="font-bold">{{ slotProps.message.summary }}</div>
                <ul
                    v-if="Array.isArray(slotProps.message.data?.errors)"
                    class="list-disc pl-5 m-0"
                >
                    <li v-for="(err, i) in slotProps.message.data.errors" :key="i">
                        {{ err }}
                    </li>
                </ul>

                <div v-else class="whitespace-pre-line">
                    {{ slotProps.message.detail }}
                </div>
            </div>
        </template>
    </Toast>
</template>

<style scoped></style>
