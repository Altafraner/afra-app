<script setup>
import UserPeek from '@/components/UserPeek.vue';
import { formatSlotId, formatStudent } from '@/helpers/formatters.ts';

const props = defineProps({
    row: { type: Object, required: true },
    profunda: { type: Array, required: true },
    editing: { type: Boolean, default: false },
});

defineEmits(['start-edit', 'save']);

const partnerFor = (partnerschaft) =>
    partnerschaft.personA.id === props.row.person.id
        ? partnerschaft.personB
        : partnerschaft.personA;

const formatDatum = (iso) => {
    if (!iso) return '?';
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
};

const zusatzInfoLines = () => {
    if (!props.row.zusatzInformation) return [];
    try {
        const parsed = JSON.parse(props.row.zusatzInformation);
        const lines = [];
        if (parsed.auslandVon || parsed.auslandBis) {
            lines.push(
                `Auslandsaufenthalt: ${formatDatum(parsed.auslandVon)} – ${formatDatum(parsed.auslandBis)}`,
            );
            if (parsed.interkulturellesEssay === true) {
                lines.push('Ersetzt Humanities durch das Interkulturelle Essay');
            } else if (parsed.interkulturellesEssay === false) {
                lines.push('Belegt Humanities regulär');
            }
        }
        if (parsed.lernvertragLehrer) {
            lines.push(`Lernvertrag mit: ${parsed.lernvertragLehrer}`);
        }
        return lines;
    } catch {
        return [props.row.zusatzInformation];
    }
};
</script>

<template>
    <span class="grid grid-cols-[19em_1fr_1fr_1fr_1fr_1fr] gap-1">
        <UserPeek :person="row.person" class="w-full min-w-0" fullSize showGroup />

        <UPopover v-if="Object.keys(row.wuensche).length > 0">
            <UButton icon="i-lucide-crown" color="info" variant="ghost" size="sm" />
            <template #content>
                <ul>
                    <li v-for="(value, key) in row.wuensche" :key="key">
                        <strong>{{ formatSlotId(key) }}</strong>
                        <ol class="list-decimal pl-6 p-3">
                            <li v-for="w in value" :key="w.id" :value="w.rang">
                                ({{ w.unprocessedRang }})
                                {{ profunda.find((p) => p.id === w.id)?.bezeichnung ?? '—' }}
                            </li>
                        </ol>
                    </li>
                </ul>
            </template>
        </UPopover>
        <span v-else></span>

        <UTooltip v-if="(row.partnerschaften?.length ?? 0) !== 0" text="Partnerschaft(en)">
            <UPopover>
                <UButton icon="i-lucide-users" color="primary" variant="ghost" size="sm" />
                <template #content>
                    <ul class="list-disc pl-4 p-3">
                        <li v-for="p in row.partnerschaften" :key="p.id">
                            {{ p.bezeichnung }}: mit {{ formatStudent(partnerFor(p)) }}
                        </li>
                    </ul>
                </template>
            </UPopover>
        </UTooltip>
        <span v-else></span>

        <UPopover v-if="row.warnings.length !== 0">
            <UButton icon="i-lucide-triangle-alert" color="warning" variant="ghost" size="sm" />
            <template #content>
                <ul class="list-disc pl-4 p-3">
                    <li v-for="w in row.warnings" :key="w">
                        {{ w.text }}
                    </li>
                </ul>
            </template>
        </UPopover>
        <span v-else></span>

        <UPopover v-if="row.zusatzInformation">
            <UButton
                color="secondary"
                icon="i-lucide-clipboard-list"
                size="sm"
                variant="ghost"
            />
            <template #content>
                <ul class="list-disc pl-4 p-3">
                    <li v-for="line in zusatzInfoLines()" :key="line">
                        {{ line }}
                    </li>
                </ul>
            </template>
        </UPopover>
        <span v-else></span>

        <UButton
            v-if="!editing"
            icon="i-lucide-pencil"
            color="neutral"
            variant="ghost"
            size="sm"
            @click="$emit('start-edit')"
        />
        <UButton
            v-else
            icon="i-lucide-check"
            color="success"
            variant="ghost"
            size="sm"
            @click="$emit('save')"
        />
    </span>
</template>
