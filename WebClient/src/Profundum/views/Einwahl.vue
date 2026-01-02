<script setup>
import { computed, ref } from 'vue';
import EinwahlSelectorGroup from '@/Profundum/components/EinwahlSelectorGroup.vue';
import EinwahlSelector from '@/Profundum/components/EinwahlSelector.vue';
import { Button, Select, useToast } from 'primevue';
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

    for (const option of options.value) {
        results.value[option.id] = [null, null, null];
    }
}

async function send() {
    console.log('Sending, ...', results);
    const api = mande('/api/profundum/sus/wuensche');

    try {
        await api.post(results.value);
    } catch (e) {
        console.log(e.response);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail:
                'Deine Belegwünsche sind fehlerhaft. \n' + (e.body ? e.body.split('(')[0] : ''),
        });
        return;
    }

    toast.add({
        severity: 'success',
        summary: 'Wünsche erfolgreich abgegeben',
        detail: 'Deine Wünsche wurden erfolgreich gespeichert.',
        life: 3000,
    });

    await router.push({ name: 'Home' });
}

const preSelected = computed(() => {
    // return an array of the ids of all selected options
    const computedResult = {};
    for (const option of options.value) {
        for (let i = 0; i < results.value[option.id].length; i++) {
            const value = results.value[option.id][i];
            const valueObj = option.options.find((opt) => opt.value === value);
            if (valueObj && valueObj.alsoIncludes && valueObj.alsoIncludes.length > 0) {
                for (const alsoIncludedElement of valueObj.alsoIncludes) {
                    if (!computedResult[alsoIncludedElement])
                        computedResult[alsoIncludedElement] = [];
                    computedResult[alsoIncludedElement][i] = valueObj;
                }
            }
        }
    }
    return computedResult;
});

const maySend = computed(() => {
    for (const option of options.value) {
        if (!results.value[option.id].every((value) => value !== null)) {
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

    <h2>Kriterien</h2>

    <p>
        Im folgenden Formular könnt ihr Wünsche für das Profundum im folgenden Halbjahr abgeben.
        Bitte beachtet das Profundarium für ausführliche Informationen zu jedem Angebot.
    </p>

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
        >. Jeder soll immer einen seiner drei Wünsche erhalten.
    </p>

    <hr class="my-3" />

    <div v-for="option in options" :key="option.id" class="mb-4">
        <h2>{{ option.label }}</h2>
        <EinwahlSelectorGroup
            v-if="!option.fixed"
            v-model="results[option.id]"
            :options="option.options"
            :pre-selected="preSelected[option.id]"
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
</template>

<style scoped></style>
