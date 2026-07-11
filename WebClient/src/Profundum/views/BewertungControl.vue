<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { computed, shallowRef } from 'vue';
import type { ProfundumFeedbackStatus } from '@/Profundum/models/feedback';
import { chooseColorNuxtUi, formatSlot } from '@/helpers/formatters';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import type { AccordionItem } from '@nuxt/ui/components/Accordion.d.vue.ts';
import { ProfundumSlot } from '@/Profundum/models/verwaltung.ts';
import BewertungControlTable from '@/Profundum/components/BewertungControl/BewertungControlTable.vue';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Feedback',
        to: {
            name: 'Profundum-Feedback-Abgeben',
        },
    },
    {
        label: 'Überwachung',
        to: {
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

interface ControlAccordionItem extends AccordionItem {
    info: ProfundumSlot;
    id: string;
    detail: ProfundumFeedbackStatus[];
    done: number;
    count: number;
}

const slots = computed(() =>
    Object.keys(control.value).map<ControlAccordionItem>((key: string) => {
        return {
            id: key,
            info: control.value[key][0].slot,
            detail: control.value[key],
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
    <UAccordion :items="slots">
        <template #trailing="{ item, open }">
            <div class="flex justify-between w-full mr-4 items-center">
                <span>
                    {{ formatSlot(item.info) }}
                </span>
                <span class="inline-flex gap-2 items-center">
                    <UButton
                        v-if="!item.info.isFeedbackPublished"
                        color="primary"
                        label="Veröffentlichen"
                        class="min-w-30 justify-center"
                        variant="subtle"
                        @click="publish($event, item.info.id, true)"
                    />
                    <UButton
                        v-else
                        color="neutral"
                        label="Veröffentlicht"
                        class="min-w-30 justify-center"
                        variant="subtle"
                        @click="publish($event, item.info.id, false)"
                    />
                    <UBadge
                        :color="chooseColorNuxtUi((100 * item.done) / item.count, 25, true)"
                        variant="soft"
                        class="min-w-12 justify-center"
                        >{{ item.done }} / {{ item.count }}</UBadge
                    >
                    <UIcon v-if="open" class="size-5" name="i-lucide-chevron-up" />
                    <UIcon v-else class="size-5" name="i-lucide-chevron-down" />
                </span>
            </div>
        </template>
        <template #content="{ item }">
            <BewertungControlTable :value="item.detail" />
        </template>
    </UAccordion>
</template>

<style scoped></style>
