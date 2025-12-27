export const routes = [
    {
        path: '/profundum/katalog',
        name: 'Profundum-Einwahl',
        component: () => import('@/Profundum/views/Einwahl.vue'),
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
];
