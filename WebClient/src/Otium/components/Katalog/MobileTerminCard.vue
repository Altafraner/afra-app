<script lang="ts" setup>
import { Button, Card, Message, Skeleton } from 'primevue';
import Termin from '@/Otium/components/Katalog/Termin.vue';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import AuslastungsTag from '@/Otium/components/Shared/AuslastungsTag.vue';
import { formatPerson } from '@/helpers/formatters';
import { ref } from 'vue';

const props = defineProps({
    termin: {
        required: true,
        type: Object,
    },
});

const emit = defineEmits(['reload']);

const open = ref(false);
</script>

<template>
    <Card :pt="{ body: { class: 'p-0' } }" class="shadow-none py-2 last:pb-0">
        <template #title>
            <div class="flex justify-between gap-2">
                <span class="inline-flex gap-3 items-center flex-auto shrink wrap-anywhere">
                    <otium-kategorie-tag
                        v-if="termin.kategorieFound"
                        :value="termin.kategorieFound"
                        hide-name
                        minimal
                    />
                    {{ termin.otium }}
                </span>
                <span class="min-w-16 shrink-0">
                    <AuslastungsTag
                        :auslastung="termin.auslastung"
                        :ist-abgesagt="termin.istAbgesagt"
                    />
                </span>
            </div>
        </template>
        <template #subtitle>
            {{ termin.ort
            }}<template v-if="termin.tutor">
                &CenterDot; {{ formatPerson(termin.tutor) }}</template
            >
        </template>
        <template v-if="open" #content>
            <Suspense>
                <Termin :termin-id="termin.id" @update="() => emit('reload')" />
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
        </template>
        <template #footer>
            <Message
                v-if="termin.istAbgesagt"
                :pt="{ content: { class: 'justify-center' } }"
                class="outline-none"
                disabled
                fluid
                severity="error"
                size="small"
                >Abgesagt</Message
            >
            <Button
                v-else-if="!open"
                fluid
                label="Mehr anzeigen"
                severity="secondary"
                size="small"
                @click="() => (open = true)"
            ></Button>
            <Button
                v-else
                fluid
                icon="pi pi-minus"
                label="Verbergen"
                severity="secondary"
                size="small"
                @click="() => (open = false)"
            ></Button>
        </template>
    </Card>
</template>

<style scoped></style>
