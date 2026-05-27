export const routes = [
    {
        path: '/otium/katalog',
        name: 'Otium-Katalog',
        component: () => import('@/Otium/views/Katalog/Index.vue'),
    },
    {
        path: '/otium/katalog/:datum',
        name: 'Otium-Katalog-Datum',
        component: () => import('@/Otium/views/Katalog/Index.vue'),
        props: true,
    },
    {
        path: '/otium/katalog/:datum/:terminId',
        name: 'Otium-Katalog-Datum-Termin',
        component: () => import('@/Otium/views/Katalog/Index.vue'),
        props: true,
    },
    {
        path: '/student/:studentId',
        name: 'Mentee',
        component: () => import('@/Otium/views/Teacher/Mentee.vue'),
        props: true,
    },
    {
        path: '/otium/management/termin/:terminId',
        name: 'Verwaltung-Termin',
        component: () => import('@/Otium/views/Management/Termin.vue'),
        props: true,
    },
    {
        path: '/otium/management',
        name: 'Verwaltung',
        component: () => import('@/Otium/views/Management/OtiaOverview.vue'),
    },
    {
        path: '/otium/management/otium/:otiumId',
        name: 'Verwaltung-Otium',
        component: () => import('@/Otium/views/Management/OtiumEdit.vue'),
        props: true,
    },
    {
        path: '/otium/management/schuljahr/neu',
        name: 'Verwaltung-Schuljahr-Neu',
        component: () => import('@/Otium/components/Schuljahr/CreateSchoolyear.vue'),
    },
];
