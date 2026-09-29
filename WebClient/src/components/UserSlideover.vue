<script lang="ts" setup>
import { UserInfoMinimal } from '@/models/user/user.ts';
import { formatPerson, formatTutor } from '@/helpers/formatters.ts';
import { nextTick, onMounted, ref, watch } from 'vue';
import { mande } from 'mande';
import { useSwipe } from '@vueuse/core';

const props = defineProps<{
    user: UserInfoMinimal;
}>();
const emit = defineEmits(['close']);

const toast = useToast();
const mentors = ref<UserInfoMinimal[]>([]);
const isLoadingMentors = ref(false);
const mentorsLoaded = ref(false);

const fetchMentors = async (id: string) => {
    if (!id) return;
    isLoadingMentors.value = true;
    try {
        const res: any = await mande(`/api/people/${id}/mentor`).get();
        mentors.value = Array.isArray(res) ? res : (res?.items ?? []);
        mentorsLoaded.value = true;
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler beim Laden',
            description: 'Mentor:innen konnten nicht geladen werden.',
            duration: 2000,
        });
    } finally {
        isLoadingMentors.value = false;
    }
};

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

if (props.user?.id) {
    fetchMentors(props.user.id);
}

onMounted(async () => {
    await nextTick();
    const content = document.querySelector<HTMLElement>('[data-slot="content"]');
    const { isSwiping, direction } = useSwipe(content);
    watch(isSwiping, () => {
        if (direction.value === 'left') {
            emit('close');
        }
    });
});
</script>

<template>
    <USlideover ref="dialog-content" :title="formatPerson(user)" side="left">
        <template #description>
            <div class="flex flex-row flex-wrap gap-2">
                <UBadge
                    v-if="user?.gruppe || user?.rolle"
                    :label="user.gruppe != '' ? user.gruppe : user.rolle"
                    color="neutral"
                    variant="soft"
                />
                <UButton
                    v-if="user?.email"
                    :label="user.email"
                    :ui="{ trailingIcon: 'size-4' }"
                    color="neutral"
                    trailing-icon="i-lucide-clipboard-copy"
                    variant="ghost"
                    @click="copy(user.email)"
                />
            </div>
        </template>
        <template #body>
            <img
                v-if="user.hasAvatar"
                :src="`/api/people/${user.id}/avatar?dimension=512`"
                :srcset="`/api/people/${user.id}/avatar?dimension=1024 2x, /api/people/${user.id}/avatar?dimension=512`"
                alt="Avatar"
                class="rounded-xl max-w-full bg-elevated"
                height="1024"
                width="1024"
            />

            <template v-if="mentorsLoaded && mentors.length">
                <USeparator class="my-2" size="sm" />
                <div class="mt-4 flex flex-col gap-2">
                    <div class="text-700 text-sm mb-2 font-medium">Mentor:innen</div>
                    <div v-for="mentor in mentors" :key="mentor.id">
                        <div>{{ formatTutor(mentor) }}</div>
                        <UButton
                            v-if="mentor.email"
                            :label="mentor.email"
                            :ui="{ trailingIcon: 'size-4' }"
                            color="neutral"
                            trailing-icon="i-lucide-clipboard-copy"
                            variant="ghost"
                            @click="copy(mentor.email)"
                        />
                    </div>
                </div>
            </template>
        </template>
    </USlideover>
</template>

<style scoped></style>
