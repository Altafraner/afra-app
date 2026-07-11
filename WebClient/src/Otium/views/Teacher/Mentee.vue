<script setup>
import { computed, ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import StudentOverview from '@/Otium/components/Overview/StudentOverview.vue';
import { formatStudent } from '@/helpers/formatters';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const props = defineProps({
    studentId: String,
});

const loading = ref(true);
const mentee = ref(null);
const user = useUser();
const toast = useToast();
const termine = ref(null);
const all = ref(false);

const username = computed(() => {
    if (user.user) {
        return formatStudent(mentee.value);
    } else {
        return null;
    }
});

const navItems = ref([
    {
        label: 'Mentees',
    },
    {
        label: username,
    },
]);

async function fetchData(getAll = false) {
    loading.value = true;
    const dataGetter = mande('/api/otium/student/' + props.studentId);
    try {
        const result = await (getAll ? dataGetter.get('all') : dataGetter.get());
        termine.value = result.termine;
        mentee.value = result.mentee;
        all.value = getAll;
    } catch (e) {
        await user.update();
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Laden der Daten aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

fetchData();
</script>

<template>
    <template v-if="!loading">
        <NavBreadcrumb :items="navItems" />
        <h1>{{ formatStudent(mentee) }}</h1>
        <h2 v-if="!all">Otium</h2>
        <p v-if="!all">Gezeigt werden die Veranstaltungen der nächsten drei Wochen.</p>
        <h2 v-if="all">Alle Veranstaltungen</h2>
        <StudentOverview :student="mentee" :termine="termine" />
        <UButton
            v-if="!all"
            class="mt-4"
            @click="fetchData(true)"
            label="Alle anzeigen"
            color="secondary"
            :loading="loading"
            variant="subtle"
        />
        <h2>Profundum</h2>
        <UButton
            :to="{
                name: 'Profundum-Feedback-Einsicht-Student',
                props: { studentId: props.studentId },
            }"
            label="Feedback Einsehen"
            color="secondary"
            variant="subtle"
        ></UButton>
    </template>
    <div class="flex gap-3" v-else>
        <h1>
            <USkeleton class="h-[1em] w-full" />
        </h1>
        <h2>
            <USkeleton class="h-[1em] w-full" />
        </h2>
        <p>
            <USkeleton class="h-[1em] w-full" />
        </p>
    </div>
</template>

<style scoped></style>
