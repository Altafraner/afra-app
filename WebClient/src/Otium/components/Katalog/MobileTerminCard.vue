<script lang="ts" setup>
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
    <UCard
        :ui="{
            header: 'p-2 sm:px-4',
            body: 'p-2 sm:px-4',
            footer: 'p-2 sm:px-4',
        }"
        class="shadow-none py-2 last:pb-0"
        variant="soft"
    >
        <template #title>
            <span class="flex justify-between gap-2">
                <span class="inline-flex gap-3 items-center flex-auto shrink wrap-anywhere">
                    <otium-kategorie-tag
                        v-if="termin.kategorieFound?.icon ?? false"
                        :value="termin.kategorieFound"
                        hide-name
                        minimal
                    />
                    <span
                        :class="{
                            'text-green-700 dark:text-green-300': termin.istEingeschrieben,
                        }"
                        >{{ termin.otium }}</span
                    >
                </span>
                <span class="min-w-16 shrink-0">
                    <AuslastungsTag
                        :auslastung="termin.auslastung"
                        :ist-abgesagt="termin.istAbgesagt"
                    />
                </span>
            </span>
        </template>
        <template #description>
            {{ termin.ort
            }}<template v-if="termin.tutor">
                &CenterDot; {{ formatPerson(termin.tutor) }}</template
            >
        </template>
        <template v-if="open" #default>
            <Suspense>
                <Termin :termin-id="termin.id" @update="() => emit('reload')" />
                <template #fallback>
                    <div>
                        <h1>
                            <USkeleton class="h-12 w-[60%]" />
                        </h1>
                        <p>
                            <USkeleton class="h-[1em] w-[40%]" />
                        </p>
                        <p class="mt-12">
                            <USkeleton class="h-8 w-[55%]" />
                        </p>
                    </div>
                </template>
            </Suspense>
        </template>
        <template #footer>
            <UButton
                v-if="termin.istAbgesagt"
                :ui="{
                    base: 'flex justify-center',
                }"
                class="w-full"
                disabled
                color="error"
                label="Abgesagt"
                size="lg"
                variant="soft"
            />
            <UButton
                v-else-if="!open"
                label="Mehr anzeigen"
                :ui="{
                    base: 'flex justify-center',
                }"
                class="w-full"
                color="neutral"
                size="lg"
                variant="soft"
                @click="
                    () => {
                        open = true;
                    }
                "
            />
            <UButton
                v-else
                :ui="{
                    base: 'flex justify-center',
                }"
                label="Verbergen"
                class="w-full"
                color="neutral"
                icon="i-lucide-minus"
                size="lg"
                variant="soft"
                @click="
                    () => {
                        open = false;
                    }
                "
            ></UButton>
        </template>
    </UCard>
</template>

<style scoped></style>
