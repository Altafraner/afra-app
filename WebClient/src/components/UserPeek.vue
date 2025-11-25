<script lang="ts" setup>
import { ref } from 'vue';
import { Button, Divider, Popover, Tag, useToast } from 'primevue';
import { mande } from 'mande';
import { formatStudent, formatTutor } from '@/helpers/formatters';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal.ts';

defineOptions({ name: 'UserPeek' });

const props = defineProps({
    showGroup: { type: Boolean, default: false },
    person: { type: Object, required: true },
    label: { type: String, default: '' },
    displayFunction: { type: Function, default: formatStudent },
});

const toast = useToast();
const pop = ref();

const copy = async (text: string) => {
    try {
        await navigator.clipboard.writeText(text);
        toast.add({
            severity: 'success',
            summary: 'Kopiert',
            detail: 'Die E-Mail-Adresse wurde in die Zwischenablage kopiert.',
            life: 2000,
        });
    } catch {
        toast.add({ severity: 'error', summary: 'Fehler beim Kopieren', life: 2000 });
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

const toggle = async (event: Event) => {
    pop.value?.toggle(event);
    if (!mentorsLoaded.value && props.person?.id) {
        await fetchMentors(props.person.id);
    }
};
</script>

<template>
    <span>
        <Button
            :label="displayFunction(person)"
            class="py-1 font-semibold"
            variant="text"
            @click="toggle($event)"
        />

        <Tag
            v-if="!person?.gruppe && person?.rolle && showGroup"
            :value="person.rolle"
            rounded
            severity="info"
        />
        <Tag
            v-if="person?.gruppe && showGroup"
            :value="person.gruppe"
            rounded
            severity="info"
        />

        <Popover
            ref="pop"
            :aria-label="displayFunction(person)"
            dismissable
            showCloseIcon
            style="min-width: 15rem"
        >
            <div class="p-3">
                <div class="flex items-center gap-3 mb-3">
                    <div class="font-bold">{{ displayFunction(person) }}</div>
                    <Tag
                        v-if="!person?.gruppe && person?.rolle"
                        :value="person.rolle"
                        rounded
                        severity="info"
                    />
                    <Tag v-if="person?.gruppe" :value="person.gruppe" rounded severity="info" />
                </div>

                <Divider />

                <Button
                    v-if="person?.email"
                    :label="person.email"
                    class="-ml-2"
                    icon="pi pi-envelope"
                    size="small"
                    variant="text"
                    @click.prevent="copy(person.email)"
                />

                <template v-if="mentorsLoaded && mentors.length">
                    <Divider />
                    <div class="mt-2 flex flex-col gap-2">
                        <div class="text-700 text-sm mb-2 font-medium">Mentor:innen</div>
                        <div v-for="mentor in mentors" :key="mentor.id">
                            <div>{{ formatTutor(mentor) }}</div>
                            <Button
                                v-if="mentor.email"
                                :label="mentor.email"
                                class="-ml-2"
                                icon="pi pi-envelope"
                                size="small"
                                variant="text"
                                @click.prevent="copy(mentor.email)"
                            />
                        </div>
                    </div>
                </template>
            </div>
        </Popover>
    </span>
</template>
