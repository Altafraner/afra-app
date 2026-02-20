<script setup>
import { useUser } from '@/stores/user';
import { defineAsyncComponent } from 'vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import FullCalendar from '@fullcalendar/vue3';
import timeGridPlugin from '@fullcalendar/timegrid';
import iCalendarPlugin from '@fullcalendar/icalendar';

const Student = defineAsyncComponent(() => import('@/Otium/views/Dashboard/Student.vue'));
const Teacher = defineAsyncComponent(() => import('@/Otium/views/Dashboard/Teacher.vue'));

const user = useUser();
const otiumStore = useOtiumStore();
await otiumStore.updateKategorien();

const calendarOptions = {
    plugins: [timeGridPlugin, iCalendarPlugin],
    initialView: 'timeGridWeek',
    slotMinTime: '12:00:00',
    slotMaxTime: '21:00:00',
    nowIndicator: true,
    expandRows: true,
    allDaySlot: false,
    weekends: false,
    events: {
        url: '/api/calendar.ics',
        format: 'ics',
    },
};
</script>

<template>
    <Student v-if="user.isStudent" />
    <Teacher v-if="user.isTeacher" />
    <FullCalendar class="mt-10" :options="calendarOptions" />
</template>

<style scoped>
p a {
    color: var(--p-primary-500);
}

p a:visited {
    color: var(--p-primary-700);
}
</style>
