<script setup>
import { Tag } from 'primevue';
import { formatTutor } from '@/helpers/formatters';
import UserPeek from '@/components/UserPeek.vue';
import {
    formatFreistellungDate,
    formatFreistellungDateRange,
    formatFreistellungTime,
    entscheidungSeverity,
    entscheidungLabel,
} from '@/Freistellung/helpers/formatters.js';

defineProps({
    /** The Freistellungsantrag object from the API */
    antrag: { type: Object, required: true },
    /** Whether to show the student's name (not needed in student's own view) */
    showStudent: { type: Boolean, default: false },
    /** Whether to render the betroffene Stunden table */
    showStunden: { type: Boolean, default: true },
    /** Whether to render the Entscheidungen list */
    showEntscheidungen: { type: Boolean, default: true },
    /** Whether to apply reduced opacity (used for processed/completed cards) */
    muted: { type: Boolean, default: false },
    /** Optional status Tag to show (e.g. { severity, value }) */
    statusTag: { type: Object, default: null },
});
</script>

<template>
    <div class="border rounded-lg p-4 shadow-sm" :class="{ 'opacity-80': muted }">
        <div class="flex items-start justify-between gap-2 mb-2">
            <div>
                <span class="font-semibold text-lg">{{ antrag.grund }}</span>
                <template v-if="showStudent">
                    <UserPeek :person="antrag.student" :showGroup="true" />
                </template>
                <Tag
                    v-if="statusTag"
                    class="ml-2"
                    :severity="statusTag.severity"
                    :value="statusTag.value"
                />
            </div>
            <div class="text-right text-sm whitespace-nowrap">
                <Tag :value="formatFreistellungDateRange(antrag.von, antrag.bis)" />
            </div>
        </div>

        <p class="text-sm mb-3">
            <span class="font-semibold">Grund:</span> {{ antrag.beschreibung }}
        </p>

        <template v-if="showStunden && antrag.betroffeneStunden?.length">
            <h4 class="font-semibold mb-1 text-sm">Betroffene Stunden:</h4>
            <table class="w-full text-sm mb-3">
                <thead>
                    <tr class="text-left border-b">
                        <th class="py-1 pr-3">Datum</th>
                        <th class="py-1 pr-3">Block</th>
                        <th class="py-1 pr-3">Fach</th>
                        <th class="py-1 ">Lehrkraft</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="s in antrag.betroffeneStunden"
                        :key="s.id"
                        class="border-b last:border-0"
                    >
                        <td class="py-1 pr-3">{{ formatFreistellungDate(s.datum) }}</td>
                        <td class="py-1 pr-3">{{ s.block }}</td>
                        <td class="py-1 pr-3">{{ s.fach }}</td>
                        <UserPeek :person="s.lehrer"/>
                    </tr>
                </tbody>
            </table>
        </template>

        <template v-if="showEntscheidungen && antrag.entscheidungen?.length">
            <h4 class="font-semibold mb-1 text-sm">Entscheidungen:</h4>
            <div class="flex flex-col gap-1 mb-2">
                <div
                    v-for="e in antrag.entscheidungen"
                    :key="e.id"
                    class="flex items-center gap-2 text-sm"
                >
                    <Tag
                        :severity="entscheidungSeverity[e.status]"
                        :value="entscheidungLabel[e.status]"
                    />
                    <span>{{ formatTutor(e.lehrer) }}</span>
                    <span v-if="e.kommentar" class="text-xs text-gray-500 italic">
                        „{{ e.kommentar }}"
                    </span>
                </div>
            </div>
        </template>
        <template v-if="antrag.sekretariatKommentar">
            <div class="mb-2 p-2 rounded border border-red-200 bg-red-50 text-sm">
                <span class="font-semibold text-red-700">Sekretariat:</span>
                <span class="ml-1">{{ antrag.sekretariatKommentar }}</span>
            </div>
        </template>

        <template v-if="antrag.schulleiterKommentar">
            <div class="mb-2 p-2 rounded border border-red-200 bg-red-50 text-sm">
                <span class="font-semibold text-red-700">Schulleiter:</span>
                <span class="ml-1">{{ antrag.schulleiterKommentar }}</span>
            </div>
        </template>
        <slot />
    </div>
</template>
