<script lang="ts" setup>
import { ref } from 'vue';
import { mande } from 'mande';
import { formatStudent, formatTutor } from '@/helpers/formatters';
import type { UserInfoMinimal } from '@/models/user/user';

defineOptions({ name: 'UserPeek' });

const props = defineProps({
    showGroup: { type: Boolean, default: false },
    person: { type: Object, required: true },
    displayFunction: { type: Function, default: formatStudent },
});

const toast = useToast();

const copy = async (text: string) => {
    try {
        await navigator.clipboard.writeText(text);
        toast.add({
            color: 'success',
            title: 'Kopiert',
            description: 'Die E-Mail-Adresse wurde in die Zwischenablage kopiert.',
            duration: 2000,
        });
    } catch {
        toast.add({ color: 'error', title: 'Fehler beim Kopieren', duration: 2000 });
    }
};

const mentors = ref<UserInfoMinimal[]>([]);
const isLoadingMentors = ref(false);
const mentorsError = ref('');
const mentorsLoaded = ref(false);

const fetchMentors = async (id: string) => {
    if (!id) return;
    isLoadingMentors.value = true;
    mentorsError.value = '';
    try {
        const res: any = await mande(`/api/people/${id}/mentor`).get();
        mentors.value = Array.isArray(res) ? res : (res?.items ?? []);
        mentorsLoaded.value = true;
    } catch (e) {
        console.error(e);
        mentorsError.value = 'Mentor:innen konnten nicht geladen werden.';
    } finally {
        isLoadingMentors.value = false;
    }
};

const onOpen = async () => {
    if (!mentorsLoaded.value && props.person?.id) {
        await fetchMentors(props.person.id);
    }
};
</script>

<template>
    <UPopover
        :aria-label="displayFunction(person)"
        dismissable
        showCloseIcon
        @update:open="onOpen"
    >
        <UButton
            class="py-1 font-semibold h-8 w-full"
            size="lg"
            v-bind="$attrs"
            variant="ghost"
        >
            <span class="inline-flex justify-between items-center gap-2 w-full min-w-0">
                <span class="min-w-0 flex-1 truncate text-center">
                    {{ displayFunction(person) }}
                </span>
                <UBadge
                    v-if="person && showGroup"
                    color="info"
                    variant="soft"
                    class="shrink-0"
                    >{{ person.gruppe ?? person.rolle }}</UBadge
                >
            </span>
        </UButton>
        <template #content>
            <div class="p-3">
                <div class="flex items-center gap-3 mb-3">
                    <div class="font-bold">{{ displayFunction(person) }}</div>
                    <UBadge
                        v-if="!person?.gruppe && person?.rolle"
                        :label="person.rolle"
                        color="info"
                        variant="soft"
                    />
                    <UBadge
                        v-else-if="person?.gruppe"
                        :label="person.gruppe"
                        color="info"
                        variant="soft"
                    />
                </div>

                <USeparator class="my-2" size="sm" />

                <UButton
                    v-if="person?.email"
                    icon="i-lucide-mail"
                    variant="ghost"
                    @click.prevent="copy(person.email)"
                    >{{ person.email }}</UButton
                >

                <template v-if="mentorsLoaded && mentors.length">
                    <USeparator class="my-2" size="sm" />
                    <div class="mt-4 flex flex-col gap-2">
                        <div class="text-700 text-sm mb-2 font-medium">Mentor:innen</div>
                        <div v-for="mentor in mentors" :key="mentor.id">
                            <div>{{ formatTutor(mentor) }}</div>
                            <UButton
                                v-if="mentor.email"
                                icon="i-lucide-mail"
                                variant="ghost"
                                @click.prevent="copy(mentor.email)"
                                >{{ mentor.email }}</UButton
                            >
                        </div>
                    </div>
                </template>
            </div>
        </template>
    </UPopover>
</template>
