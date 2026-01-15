<script setup>
import { formatDate } from '@/helpers/formatters';
import {
    Accordion,
    AccordionContent,
    AccordionHeader,
    AccordionPanel,
    Badge,
    Button,
    Column,
    DataTable,
    Message,
} from 'primevue';
import { useUser } from '@/stores/user';
import { computed } from 'vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { findPath } from '@/helpers/tree.js';
import AfraKategorieTag from '@/Otium/components/Shared/AfraKategorieTag.vue';
import { marked } from 'marked';
import AfraOtiumAnwesenheit from '@/Otium/components/Shared/AfraOtiumAnwesenheit.vue';

const props = defineProps({
    termine: Array,
    showKatalog: Boolean,
    student: {
        type: Object,
        required: false,
    },
});
const user = useUser();
const otiumStore = useOtiumStore();

const findKategorie = (kategorie) => {
    const path = findPath(otiumStore.kategorien, kategorie);
    for (const element of path) {
        if (element.icon != null) {
            return element;
        }
    }
    return path[0];
};

const formatedEnrollments = computed(() => {
    return props.termine.map((week) => {
        let lastBlock = null;
        let lastDate = null;
        const result = week.einschreibungen.map((enrollment) => {
            const datumVisible = enrollment.datum !== lastDate;
            const blockVisible = datumVisible || enrollment.block !== lastBlock;
            const innerResult = {
                datum: enrollment.datum,
                datumVisible: datumVisible,
                block: enrollment.block,
                blockVisible: blockVisible,
                otium: enrollment.otium,
                ort: enrollment.ort,
                terminId: enrollment.terminId,
                kategorieId: enrollment.kategorieId,
                kategorie: enrollment.kategorieId
                    ? findKategorie(enrollment.kategorieId)
                    : null,
                anwesenheit: enrollment.anwesenheit,
            };
            lastDate = enrollment.datum;
            lastBlock = enrollment.block;
            return innerResult;
        });

        return {
            ...week,
            messageHtml: week.message ? marked(week.message, { sanitize: true }) : null,
            einschreibungen: result,
        };
    });
});

const isOs = computed(() => {
    if (props.student) {
        return props.student.rolle === 'Oberstufe';
    }
    return user.user.rolle === 'Oberstufe';
});
</script>

<template>
    <Accordion v-if="termine != null">
        <AccordionPanel
            v-for="termin in formatedEnrollments"
            :key="termin.monday"
            :value="termin.monday"
        >
            <AccordionHeader>
                <div class="flex w-full justify-between mr-4">
                    <span>
                        {{ formatDate(new Date(termin.monday)) }} –
                        {{
                            formatDate(new Date(new Date(termin.monday).getTime() + 518400000))
                        }}
                    </span>
                    <span v-if="!isOs" class="flex flex-row gap-3">
                        <Badge v-if="!termin.message" class="w-[8rem]" severity="secondary"
                            >Ok</Badge
                        >
                        <Badge v-else class="w-[8rem]" severity="danger">Offen</Badge>
                    </span>
                </div>
            </AccordionHeader>
            <AccordionContent>
                <Message v-if="termin.message && !isOs" class="mb-2" severity="warn">
                    <div v-html="termin.messageHtml" />
                </Message>
                <Message v-else-if="!isOs" class="mb-2" severity="success">
                    <div>Die Einschreibungen entsprechen den Vorgaben.</div>
                </Message>

                <DataTable :value="termin.einschreibungen" size="small">
                    <Column header="Wochentag">
                        <template #body="{ data }">
                            <template v-if="data.datumVisible">
                                {{ formatDate(new Date(data.datum)) }}
                            </template>
                        </template>
                    </Column>
                    <Column header="Block">
                        <template #body="{ data }">
                            <template v-if="data.blockVisible">
                                {{ data.block }}
                            </template>
                        </template>
                    </Column>
                    <Column header="Angebot">
                        <template #body="{ data }">
                            <span v-if="data.otium">
                                <AfraKategorieTag
                                    v-if="data.kategorieId"
                                    :value="findKategorie(data.kategorieId)"
                                    hide-name
                                    minimal
                                />
                                <Button
                                    :label="data.otium"
                                    :to="{
                                        name: 'Katalog-Datum-Termin',
                                        params: { datum: data.datum, terminId: data.terminId },
                                    }"
                                    as="RouterLink"
                                    variant="text"
                                />
                            </span>
                            <Button
                                v-else-if="props.showKatalog"
                                :to="{ name: 'Katalog-Datum', params: { datum: data.datum } }"
                                as="RouterLink"
                                class="w-full justify-start"
                                icon="pi pi-list"
                                label="Katalog"
                                size="small"
                            />
                            <span v-else>Keine Einschreibung</span>
                        </template>
                    </Column>
                    <Column header="Ort">
                        <template #body="{ data }">
                            <template v-if="data.ort">
                                {{ data.ort }}
                            </template>
                        </template>
                    </Column>
                    <Column>
                        <template #body="{ data }">
                            <AfraOtiumAnwesenheit
                                v-if="data.anwesenheit"
                                v-model="data.anwesenheit"
                                minimal
                            />
                        </template>
                    </Column>
                    <template #empty>
                        <div class="flex justify-center">Keine Einträge</div>
                    </template>
                </DataTable>
            </AccordionContent>
        </AccordionPanel>
    </Accordion>
</template>

<style scoped></style>
