<script lang="ts" setup>
import { computed, ref } from 'vue';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';
import { formatTutor } from '@/helpers/formatters';
import UserPeek from '@/components/UserPeek.vue';
import {
    formatFreistellungDate,
    formatFreistellungTime,
    formatFreistellungDateRange,
    entscheidungColor,
    entscheidungLabel,
    statusColor,
    statusLabel,
} from '@/Freistellung/helpers/formatters';

const props = withDefaults(
    defineProps<{
        /** The Freistellungsantrag object from the API */
        antrag: Freistellungsantrag;
        /** Whether to show the student's name (not needed in student's own view) */
        showStudent?: boolean;
        /** Whether to render the betroffene Stunden table */
        showStunden?: boolean;
        /** Whether to render the Entscheidungen list */
        showEntscheidungen?: boolean;
        /** Whether to apply reduced opacity (used for processed/completed cards) */
        muted?: boolean;
        /** Whether to show a Badge for the overall antrag status (derived from antrag.status) */
        showStatus?: boolean;
        /** Color of the date-range Badge; defaults to "warning" for open cards and "neutral" for muted ones */
        dateTagColor?: string | null;
    }>(),
    {
        showStudent: false,
        showStunden: true,
        showEntscheidungen: true,
        muted: false,
        showStatus: false,
        dateTagColor: null,
    },
);

const resolvedDateTagColor = computed(
    () => props.dateTagColor ?? (props.muted ? 'neutral' : 'warning'),
);

const showVerlauf = ref(false);
</script>

<template>
    <UCard :class="{ 'opacity-80': muted }" :ui="{ body: 'flex flex-col gap-3' }">
        <div class="flex items-start justify-between gap-2">
            <div>
                <span class="font-semibold text-lg">{{ antrag.grund }}</span>
                <template v-if="showStudent">
                    <UserPeek :person="antrag.student" :showGroup="true" />
                </template>
                <UBadge
                    v-if="showStatus"
                    class="ml-2"
                    :color="statusColor[antrag.status]"
                    :label="statusLabel[antrag.status]"
                />
            </div>
            <div class="text-right text-sm whitespace-nowrap">
                <UBadge
                    :color="resolvedDateTagColor"
                    :label="formatFreistellungDateRange(antrag.von, antrag.bis)"
                />
            </div>
        </div>

        <p class="text-sm">
            <span class="font-semibold">Grund:</span> {{ antrag.beschreibung }}
        </p>

        <p v-if="antrag.statistik" class="text-xs text-muted">
            {{ antrag.statistik.anzahlAntraegeSchuljahr }} genehmigte Anträge und
            {{ antrag.statistik.anzahlStundenSchuljahr }} freigestellte Stunden
            {{ showStudent ? `für ${antrag.student.vorname}` : '' }} in diesem Schuljahr.
        </p>

        <template v-if="showStunden && antrag.betroffeneStunden?.length">
            <div>
                <h4 class="font-semibold mb-1 text-sm">Betroffene Stunden:</h4>
                <table class="w-full text-sm">
                    <thead>
                        <tr class="text-left border-b border-default">
                            <th class="py-1 pr-3">Datum</th>
                            <th class="py-1 pr-3">Block</th>
                            <th class="py-1 pr-3">Fach</th>
                            <th class="py-1">Lehrkraft</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="s in antrag.betroffeneStunden"
                            :key="s.id"
                            class="border-b border-default last:border-0"
                        >
                            <td class="py-1 pr-3">{{ formatFreistellungDate(s.datum) }}</td>
                            <td class="py-1 pr-3">{{ s.block }}</td>
                            <td class="py-1 pr-3">{{ s.fach }}</td>
                            <td class="py-1">
                                <UserPeek :person="s.lehrer" :display-function="formatTutor" />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </template>

        <template v-if="showEntscheidungen && antrag.entscheidungen?.length">
            <div>
                <h4 class="font-semibold mb-1 text-sm">Entscheidungen:</h4>
                <div class="flex flex-col gap-1">
                    <div
                        v-for="e in antrag.entscheidungen"
                        :key="e.id"
                        class="flex items-center gap-2 text-sm"
                    >
                        <UBadge
                            :color="entscheidungColor[e.status]"
                            :label="entscheidungLabel[e.status]"
                        />
                        <span>{{ formatTutor(e.lehrer) }}</span>
                        <span v-if="e.kommentar" class="text-xs text-muted italic">
                            „{{ e.kommentar }}"
                        </span>
                    </div>
                </div>
            </div>
        </template>

        <UAlert
            v-if="antrag.elternbestaetigungHinweis && !antrag.elternbestaetigungVorhanden"
            color="warning"
            variant="soft"
            title="Hinweis des Sekretariats zur Elternbestätigung"
            :description="antrag.elternbestaetigungHinweis"
        />

        <UAlert
            v-if="antrag.schulleiterKommentar"
            color="warning"
            variant="soft"
            title="Kommentar des Schulleiters"
            :description="antrag.schulleiterKommentar"
        />

        <div v-if="antrag.verlauf?.length">
            <UButton
                :label="showVerlauf ? 'Verlauf ausblenden' : 'Verlauf anzeigen'"
                :icon="showVerlauf ? 'i-lucide-chevron-up' : 'i-lucide-chevron-down'"
                color="neutral"
                variant="ghost"
                size="xs"
                class="px-0"
                @click="showVerlauf = !showVerlauf"
            />
            <div v-if="showVerlauf" class="flex flex-col gap-1 mt-2 text-xs text-muted">
                <div
                    v-for="(eintrag, index) in antrag.verlauf"
                    :key="index"
                    class="flex flex-wrap gap-x-2"
                >
                    <span class="whitespace-nowrap">
                        {{ formatFreistellungDate(eintrag.zeitpunkt) }}
                        {{ formatFreistellungTime(eintrag.zeitpunkt) }}
                    </span>
                    <span class="font-medium">{{ statusLabel[eintrag.neuerStatus] }}</span>
                    <span v-if="eintrag.person">– {{ formatTutor(eintrag.person) }}</span>
                    <span v-if="eintrag.kommentar" class="italic"
                        >„{{ eintrag.kommentar }}"</span
                    >
                </div>
            </div>
        </div>

        <slot />
    </UCard>
</template>
