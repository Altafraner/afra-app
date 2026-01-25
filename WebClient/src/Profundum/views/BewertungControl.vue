<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { computed, shallowRef } from 'vue';
import type { ProfundumFeedbackStatus } from '@/Profundum/models/feedback';
import { Column, DataTable, Tag } from 'primevue';
import Accordion from 'primevue/accordion';
import AccordionPanel from 'primevue/accordionpanel';
import AccordionHeader from 'primevue/accordionheader';
import AccordionContent from 'primevue/accordioncontent';
import { chooseSeverity, formatSlot, formatStudent } from '@/helpers/formatters';

const feedbackService = useFeedback();

const control = shallowRef<Record<string, ProfundumFeedbackStatus[]>>();
control.value = await feedbackService.getControl();
const slots = computed(() =>
    Object.keys(control.value).map((key) => {
        return {
            id: key,
            info: control.value[key][0].slot,
            content: control.value[key],
            done: control.value[key].reduce(
                (total, x) => (x.status == 'Done' ? total + 1 : total),
                0,
            ),
            count: control.value[key].length,
        };
    }),
);
</script>

<template>
    <Accordion lazy>
        <AccordionPanel v-for="slot in slots" :key="slot.id" :value="slot.id">
            <AccordionHeader>
                <div class="flex justify-between w-full mr-4">
                    <span>
                        {{ formatSlot(slot.info) }}
                    </span>
                    <span
                        ><Tag
                            :severity="chooseSeverity((100 * slot.done) / slot.count, 25, true)"
                            >{{ slot.done }} / {{ slot.count }}</Tag
                        ></span
                    >
                </div>
            </AccordionHeader>
            <AccordionContent>
                <DataTable :value="slot.content">
                    <Column header="Profundum">
                        <template #body="{ data }">
                            {{ data.instanz.profundumInfo.bezeichnung }}
                        </template>
                    </Column>
                    <Column header="Verantwortliche">
                        <template #body="{ data }">
                            {{
                                data.instanz.verantwortlicheInfo
                                    .map((v) => formatStudent(v, true))
                                    .join(', ')
                            }}
                        </template>
                    </Column>
                    <Column header="Status">
                        <template #body="{ data }">
                            <Tag v-if="data.status === 'Done'" severity="success"
                                >Abgeschlossen</Tag
                            >
                            <Tag v-else-if="data.status === 'Partial'" severity="warn"
                                >Teilweise</Tag
                            >
                            <Tag v-else severity="danger">Ausstehend</Tag>
                        </template>
                    </Column>
                </DataTable>
            </AccordionContent>
        </AccordionPanel>
    </Accordion>
</template>

<style scoped></style>
