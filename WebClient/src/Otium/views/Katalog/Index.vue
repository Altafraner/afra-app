<script setup>
import { computed, h, shallowRef, watch } from 'vue';
import OtiumDateSelector from '@/Otium/components/Form/OtiumDateSelector.vue';
import OtiumKategorySelector from '@/Otium/components/Form/OtiumKategorySelector.vue';
import OtiumKatalog from '@/Otium/components/Katalog/OtiumKatalog.vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import { useRoute, useRouter } from 'vue-router';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { formatDate, formatStudent } from '@/helpers/formatters';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import EditSupervisorsForm from '@/Otium/components/Schuljahr/EditSupervisorsForm.vue';
import USkeleton from '@nuxt/ui/components/Skeleton.vue';

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
const overlay = useOverlay();

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
            to: {
                name: 'Otium-Katalog',
            },
        },
        {
            label: 'Katalog',
            to: {
                name: 'Otium-Katalog',
            },
        },
    ];
    return date.value == null
        ? start
        : [
              ...start,
              {
                  label: formatDate(new Date(date.value)),
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
            if (propDate !== undefined) date.value = props.datum;
            else {
                date.value = dateDefault.value.datum;
                await router.replace({ name: 'Otium-Katalog' });
            }
        } else {
            date.value = dateDefault.value.datum;
        }
        await dateChanged();
        await kategoriesPromise;
    } catch (error) {
        console.error(error);
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        await user.update();
    }
    loading.value = false;
}

watch(props, async () => {
    if (!props.datum || (props.datum === '' && date.value !== dateDefault.value.datum)) {
        date.value = dateDefault.value.datum;
        await dateChanged();
    }
});

async function getTermine() {
    await settings.updateSchuljahr();
    datesAvailable.value = settings.schuljahr;
    dateDefault.value = settings.defaultDay;
    date.value = settings.defaultDay.datum;
}

async function getAngebote() {
    const api = mande('/api/otium');
    const result = await api.get(`${date.value}`);
    blocks.value = result.blocks;
    hinweise.value = result.hinweise;
}

async function dateChanged() {
    try {
        await getAngebote();
    } catch (error) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        if (location.name !== 'Otium-Katalog') {
            await router.replace({ name: 'Otium-Katalog' });
            await dateChanged();
        }
    }
}

function selectToday() {
    date.value = dateDefault.value.datum;
    dateChanged();
}

watch([date], () => {
    if (!loading.value && date.value != null)
        router.push({
            name: 'Otium-Katalog-Datum',
            params: {
                datum: date.value,
            },
        });
});

const blocksFiltered = computed(() => {
    if (kategorie.value == null) {
        return blocks.value;
    }
    const kategorieId = kategorie.value.id;
    return blocks.value.map((b) => {
        return {
            block: b.block,
            previews: b.previews.filter((p) => p.kategorien.includes(kategorieId)),
        };
    });
});

async function editSupervisor(blockId) {
    const form = overlay.create(EditSupervisorsForm);
    await form.open({
        date: date.value,
        blockId: blockId,
    });
    await getAngebote();
}

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
                <UAlert
                    v-if="hinweise.length === 0"
                    color="success"
                    description="Deine Belegung entspricht den Vorgaben."
                    title="Geschafft!"
                    variant="subtle"
                />
                <UAlert
                    v-else
                    color="warning"
                    title="Deine Belegung entspricht noch nicht den Vorgaben."
                    variant="subtle"
                >
                    <template #description>
                        <ul>
                            <li v-for="(item, index) in hinweise" :key="index">{{ item }}</li>
                        </ul>
                    </template>
                </UAlert>
            </template>

            <APanel
                v-for="block in blocksFiltered"
                :key="block.block.id"
                :label="block.block.name"
                class="w-auto flex-1"
                default-open
            >
                <div class="mb-4 grid grid-cols-[1fr_auto] gap-2">
                    <span>
                        Aufsicht:
                        {{
                            block.block.supervisors.length > 0
                                ? block.block.supervisors
                                      .map((s) => formatStudent(s))
                                      .join(', ')
                                : 'keine'
                        }}
                    </span>
                    <span v-if="user.isOtiumsverantwortlich">
                        <UButton
                            aria-label="Aufsichten bearbeiten"
                            class="mr-1"
                            color="secondary"
                            icon="i-lucide-pencil"
                            variant="ghost"
                            @click="() => editSupervisor(block.block.id)"
                        />
                    </span>
                    <span v-else />
                </div>
                <OtiumKatalog
                    :otia="block.previews"
                    :termin-id="terminId"
                    @reload="getAngebote"
                />
            </APanel>
            <div v-if="blocks.length === 0" class="flex justify-center mt-4">
                Keine Angebote verfügbar.
            </div>
        </template>
        <div v-else class="flex gap-5 flex-col">
            <div class="flex gap-3 justify-between">
                <USkeleton class="h-12 w-[65%]" />
                <USkeleton class="h-12 w-[33&]" />
            </div>
            <USkeleton class="h-12 w-full" />
            <UTable
                :columns="[
                    {
                        id: 'a',
                        header: h(USkeleton, { class: 'h-4 w-full' }),
                        cell: () => h(USkeleton, { class: 'h-4 w-full' }),
                    },
                    {
                        id: 'b',
                        header: h(USkeleton, { class: 'h-4 w-full' }),
                        cell: () => h(USkeleton, { class: 'h-4 w-full' }),
                    },
                    {
                        id: 'b',
                        header: h(USkeleton, { class: 'h-4 w-full' }),
                        cell: () => h(USkeleton, { class: 'h-4 w-full' }),
                    },
                ]"
                :data="new Array(4)"
            />
        </div>
    </div>
</template>

<style scoped></style>
