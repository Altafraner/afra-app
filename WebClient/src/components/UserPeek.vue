<script setup>
import { ref } from 'vue';
import { Button } from 'primevue';
import { Popover } from 'primevue';
import { Tag } from 'primevue';
import { Divider } from 'primevue';
import { useToast } from 'primevue';
import { mande } from 'mande';
import { formatStudent } from '@/helpers/formatters.js';

defineOptions({ name: 'UserPeek' });

const props = defineProps({
    person: { type: Object, required: true },
    label: { type: String, default: '' },
});

const toast = useToast();
const pop = ref();

const copy = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        toast.add({ severity: 'success', summary: 'Kopiert', detail: text, life: 2000 });
    } catch {
        toast.add({ severity: 'error', summary: 'Fehler beim Kopieren', life: 2000 });
    }
};

const mentors = ref([]);
const isLoadingMentors = ref(false);
const mentorsError = ref('');
const mentorsLoaded = ref(false);

const fetchMentors = async (id) => {
    if (!id) return;
    isLoadingMentors.value = true;
    mentorsError.value = '';
    try {
        const res = await mande(`/api/people/${id}/mentor`).get();
        mentors.value = Array.isArray(res) ? res : (res?.items ?? []);
        mentorsLoaded.value = true;
    } catch (e) {
        console.error(e);
        mentorsError.value = 'Mentor:innen konnten nicht geladen werden.';
    } finally {
        isLoadingMentors.value = false;
    }
};

const toggle = async (event) => {
    pop.value?.toggle(event);
    if (!mentorsLoaded.value && props.person?.id) {
        await fetchMentors(props.person.id);
    }
};
</script>

<template>
    <span>
        <Button
            :label="formatStudent(props.person)"
            link
            class="p-0 font-semibold"
            @click="toggle($event)"
        />

        <Popover ref="pop" :dismissable="true" :showCloseIcon="true" style="min-width: 15rem">
            <div class="p-3">
                <div class="flex align-items-center gap-3 mb-3">
                    <div class="font-bold">{{ formatStudent(person) }}</div>
                    <Tag v-if="person?.rolle" :value="person.rolle" severity="info" rounded />
                    <Tag v-if="person?.gruppe" :value="person.gruppe" severity="info" rounded />
                </div>

                <Divider />

                <div v-if="person?.email" class="mb-2">
                    <i class="pi pi-envelope mr-2"></i>
                    <a href="#" @click.prevent="copy(person.email)">{{ person.email }}</a>
                </div>

                <template v-if="mentorsLoaded && mentors.length">
                    <Divider />
                    <div class="mt-2">
                        <div class="text-700 text-sm mb-2 font-medium">Mentor:innen</div>
                        <ul class="list-none p-0 m-0 flex flex-column gap-2">
                            <li v-for="m in mentors" :key="m.id">
                                <UserPeek :person="m" />
                            </li>
                        </ul>
                    </div>
                </template>
            </div>
        </Popover>
    </span>
</template>
