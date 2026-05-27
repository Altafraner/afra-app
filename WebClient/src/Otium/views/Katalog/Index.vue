<script setup>
import { computed, shallowRef, watch } from 'vue';
import { Column, DataTable, Message, Panel, Skeleton, useToast } from 'primevue';
import OtiumDateSelector from '@/Otium/components/Form/OtiumDateSelector.vue';
import OtiumKategorySelector from '@/Otium/components/Form/OtiumKategorySelector.vue';
import OtiumKatalog from '@/Otium/components/Katalog/OtiumKatalog.vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import { useRoute, useRouter } from 'vue-router';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { formatDate } from '@/helpers/formatters';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const props = defineProps({
    datum: {
        type: String,
        required: false,
        default: '',
    },
    terminId: {
        type: String,
        required: false,
        default: undefined,
    },
});
const router = useRouter();
const location = useRoute();
const toast = useToast();
const settings = useOtiumStore();
const user = useUser();

const loading = shallowRef(true);
const datesAvailable = shallowRef([]);
const dateDefault = shallowRef(null);
const blocks = shallowRef([]);
const hinweise = shallowRef([]);
const date = shallowRef(null);
const kategorie = shallowRef(null);

const navItems = computed(() => {
    const start = [
        {
            label: 'Otium',
            route: {
                name: 'Otium-Katalog',
            },
        },
        {
            label: 'Katalog',
            route: {
                name: 'Otium-Katalog',
            },
        },
    ];
    return date.value == null
        ? start
        : [
              ...start,
              {
                  label: formatDate(new Date(date.value.datum)),
              },
          ];
});

async function startup() {
    loading.value = true;
    const terminePromise = getTermine();
    const kategoriesPromise = settings.updateKategorien();
    try {
        await terminePromise;
        if (props.datum && props.datum !== '') {
            const propDate = datesAvailable.value.find((e) => e.datum === props.datum);
            if (propDate !== undefined)
                date.value = datesAvailable.value.find((e) => e.datum === props.datum);
            else {
                date.value = dateDefault.value;
                await router.replace({ name: 'Otium-Katalog' });
            }
        } else {
            date.value = dateDefault.value;
        }
        await dateChanged();
        await kategoriesPromise;
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        await user.update();
    }
    loading.value = false;
}

watch(props, async () => {
    if (!props.datum || (props.datum === '' && date.value.datum !== dateDefault.value.datum)) {
        date.value = dateDefault.value;
        await dateChanged();
    }
});

async function getTermine() {
    await settings.updateSchuljahr();
    datesAvailable.value = settings.schuljahr;
    dateDefault.value = settings.defaultDay;
    date.value = settings.defaultDay;
}

async function getAngebote() {
    const api = mande('/api/otium');
    const result = await api.get(`${date.value.datum}`);
    blocks.value = result.blocks;
    hinweise.value = result.hinweise;
}

async function dateChanged() {
    try {
        await getAngebote();
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        if (location.name !== 'Otium-Katalog') {
            await router.replace({ name: 'Otium-Katalog' });
            await dateChanged();
        }
    }
}

function selectToday() {
    date.value = dateDefault.value;
    dateChanged();
}

watch([date], () => {
    if (!loading.value && date.value != null)
        router.push({
            name: 'Otium-Katalog-Datum',
            params: {
                datum: date.value.datum,
            },
        });
});

const blocksFiltered = computed(() => {
    if (kategorie.value == null || Object.keys(kategorie.value).length === 0) {
        return blocks.value;
    }
    const kategorieId = Object.keys(kategorie.value)[0];
    return blocks.value.map((b) => {
        return {
            block: b.block,
            previews: b.previews.filter((p) => p.kategorien.includes(kategorieId)),
        };
    });
});

startup();
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>Otia-Katalog</h1>

    <div class="flex gap-3 flex-col">
        <template v-if="!loading">
            <OtiumDateSelector
                v-model="date"
                :options="datesAvailable"
                @dateChanged="dateChanged"
                @today="selectToday"
            />
            <OtiumKategorySelector v-model="kategorie" :options="settings.kategorien" />

            <template v-if="user.isStudent && user.user.rolle !== 'Oberstufe'">
                <Message v-if="hinweise.length === 0" severity="success">
                    Deine Belegung entspricht den Vorgaben.
                </Message>
                <Message v-else severity="warn">
                    <div class="flex flex-col">
                        <div class="font-bold">
                            Deine Belegung entspricht noch nicht den Vorgaben.
                        </div>
                        <ul>
                            <li v-for="(item, index) in hinweise" :key="index">{{ item }}</li>
                        </ul>
                    </div>
                </Message>
            </template>

            <panel
                v-for="block in blocksFiltered"
                :key="block.block.id"
                :header="block.block.name"
                class="w-auto flex-1"
                toggleable
            >
                <OtiumKatalog
                    :otia="block.previews"
                    :termin-id="terminId"
                    @reload="getAngebote"
                />
            </panel>
            <div v-if="blocks.length === 0" class="flex justify-center mt-4">
                Keine Angebote verfügbar.
            </div>
        </template>
        <div v-else class="flex gap-5 flex-col">
            <div class="flex gap-3 justify-between">
                <Skeleton width="65%" height="3rem" />
                <Skeleton width="33%" height="3rem" />
            </div>
            <Skeleton width="100%" height="3rem" />
            <DataTable :value="new Array(4)">
                <Column>
                    <template #header>
                        <Skeleton />
                    </template>
                    <template #body>
                        <Skeleton />
                    </template>
                </Column>
                <Column>
                    <template #header>
                        <Skeleton />
                    </template>
                    <template #body>
                        <Skeleton />
                    </template>
                </Column>
                <Column>
                    <template #header>
                        <Skeleton />
                    </template>
                    <template #body>
                        <Skeleton />
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
</template>

<style scoped></style>
