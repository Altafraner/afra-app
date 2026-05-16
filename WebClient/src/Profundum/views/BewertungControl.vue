<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { computed, shallowRef } from 'vue';
import type { ProfundumFeedbackStatus } from '@/Profundum/models/feedback';
import { Button, Column, DataTable, Tag } from 'primevue';
import Accordion from 'primevue/accordion';
import AccordionPanel from 'primevue/accordionpanel';
import AccordionHeader from 'primevue/accordionheader';
import AccordionContent from 'primevue/accordioncontent';
import { chooseSeverity, formatSlot, formatStudent } from '@/helpers/formatters';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';

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
        label: 'Überwachung',
        route: {
            name: 'Profundum-Feedback-Control',
        },
    },
];

const feedbackService = useFeedback();

const control = shallowRef<Record<string, ProfundumFeedbackStatus[]>>({});

async function update() {
    control.value = await feedbackService.getControl();
}
await update();

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

async function publish(evt: Event, id: string, status: boolean) {
    evt.stopPropagation();
    await feedbackService.publishSlotFeedback(id, status);
    await update();
}
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <h1>Feedback Überwachung</h1>
    <Accordion lazy>
        <AccordionPanel v-for="slot in slots" :key="slot.id" :value="slot.id">
            <AccordionHeader>
                <div class="flex justify-between w-full mr-4">
                    <span>
                        {{ formatSlot(slot.info) }}
                    </span>
                    <span class="inline-flex gap-2">
                        <Button
                            v-if="!slot.info.isFeedbackPublished"
                            label="Veröffentlichen"
                            severity="primary"
                            size="small"
                            variant="text"
                            @click="publish($event, slot.info.id, true)"
                        />
                        <Button
                            v-else
                            label="Veröffentlicht"
                            severity="secondary"
                            size="small"
                            variant="text"
                            @click="publish($event, slot.info.id, false)"
                        />
                        <Tag
                            :severity="chooseSeverity((100 * slot.done) / slot.count, 25, true)"
                            >{{ slot.done }} / {{ slot.count }}</Tag
                        >
                    </span>
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
                                    .map((v: UserInfoMinimal) => formatStudent(v, true))
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
