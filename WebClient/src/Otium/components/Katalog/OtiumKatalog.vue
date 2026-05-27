<script setup>
import { Button, Column, DataTable, Divider, Skeleton } from 'primevue';
import { formatPerson } from '@/helpers/formatters';
import AuslastungsTag from '@/Otium/components/Shared/AuslastungsTag.vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import Termin from '@/Otium/components/Katalog/Termin.vue';
import { computed, ref } from 'vue';
import MobileSwitch from '@/components/MobileSwitch.vue';
import MobileTerminCard from '@/Otium/components/Katalog/MobileTerminCard.vue';

const props = defineProps({
    otia: Array,
    terminId: {
        type: String,
        required: false,
        default: undefined,
    },
});

const emit = defineEmits(['reload']);

const settings = useOtiumStore();
const rowsExpanded = ref({});
if (props.terminId) {
    rowsExpanded.value[props.terminId] = true;
}

function findKategorie(otium) {
    return settings.kategorien.find((k) => otium.kategorien.includes(k.id));
}

function expand(id) {
    let isOpen = rowsExpanded.value[id] ?? false;
    let temp = {};
    if (!isOpen) temp[id] = true;
    rowsExpanded.value = temp;
}

const processedOtia = computed(() => {
    return props.otia.map((ot) => {
        return Object.assign(
            {
                kategorieFound: findKategorie(ot),
            },
            ot,
        );
    });
});
</script>

<template>
    <MobileSwitch>
        <template #large>
            <DataTable
                v-model:expanded-rows="rowsExpanded"
                :value="processedOtia"
                data-key="id"
            >
                <Column header="Bezeichnung">
                    <template #body="{ data }">
                        <Button v-if="data.istAbgesagt" disabled variant="text">
                            <otium-kategorie-tag
                                v-if="data.kategorieFound"
                                :value="data.kategorieFound"
                                hide-name
                                minimal
                            />
                            <span class="font-semibold text-left">{{ data.otium }}</span>
                        </Button>
                        <Button
                            v-else
                            :disabled="data.istAbgesagt"
                            :label="data.otium"
                            :severity="data.istEingeschrieben ? 'success' : 'primary'"
                            variant="text"
                            @click="() => expand(data.id)"
                        >
                            <i
                                :class="{
                                    'pi-angle-down': rowsExpanded[data.id] ?? false,
                                    'pi-angle-right': !(rowsExpanded[data.id] ?? false),
                                }"
                                class="pi text-lg w-4"
                            />
                            <otium-kategorie-tag
                                v-if="data.kategorieFound?.icon ?? false"
                                :value="data.kategorieFound"
                                class="w-4"
                                hide-name
                                minimal
                            />
                            <span class="font-semibold text-left">{{ data.otium }}</span>
                        </Button>
                    </template>
                </Column>
                <Column field="ort" header="Raum" />
                <Column header="Betreuer:in">
                    <template #body="{ data }">
                        {{ data.tutor ? formatPerson(data.tutor) : '' }}
                    </template>
                </Column>
                <Column header="Auslastung">
                    <template #body="{ data }">
                        <div class="w-24">
                            <AuslastungsTag
                                :auslastung="data.auslastung"
                                :ist-abgesagt="data.istAbgesagt"
                            />
                        </div>
                    </template>
                </Column>
                <template #expansion="{ data }">
                    <div class="w-full px-4">
                        <Suspense>
                            <Termin :termin-id="data.id" @update="() => emit('reload')" />
                            <template #fallback>
                                <div>
                                    <h1>
                                        <Skeleton height="3rem" width="60%" />
                                    </h1>
                                    <p>
                                        <Skeleton width="40%" />
                                    </p>
                                    <h3 class="mt-12">
                                        <Skeleton height="2rem" width="55%" />
                                    </h3>
                                </div>
                            </template>
                        </Suspense>
                    </div>
                </template>
                <template #empty>
                    <div class="flex justify-center">Keine Angebote verfügbar.</div>
                </template>
            </DataTable>
        </template>
        <template #small>
            <template v-for="(termin, i) in processedOtia" :key="termin.id">
                <MobileTerminCard :termin="termin" @reload="() => emit('reload')" />
                <Divider v-if="i !== processedOtia.length - 1" />
            </template>
        </template>
    </MobileSwitch>
</template>

<style scoped></style>
