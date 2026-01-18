export const routes = [
    {
        path: '/profundum/katalog',
        name: 'Profundum-Einwahl',
        component: () => import('@/Profundum/views/Einwahl.vue'),
    },
    {
        path: '/profundum/management',
        name: 'Profundum-Verwaltung',
        component: () => import('@/Profundum/views/ProfundumManagement.vue'),
    },
    {
        path: '/profundum/matching',
        name: 'Profundum-Matching',
        component: () => import('@/Profundum/views/Matching.vue'),
        meta: { fullWidth: true },
    },
    {
        path: '/profundum/management/:profundumId',
        name: 'Profundum-Edit',
        component: () => import('@/Profundum/views/ProfundumEdit.vue'),
        props: true,
    },
    {
        path: '/profundum/feedback/kriterien',
        name: 'Profundum-Feedback-Kriterien',
        component: () => import('@/Profundum/views/Kriterien.vue'),
    },
    {
        path: '/profundum/feedback/abgeben',
        name: 'Profundum-Feedback-Abgeben',
        component: () => import('@/Profundum/views/Bewertung.vue'),
    },
    {
        path: '/profundum/feedback/control',
        name: 'Profundum-Feedback-Control',
        component: () => import('@/Profundum/views/BewertungControl.vue'),
    },
];
