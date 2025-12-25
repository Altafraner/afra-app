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
];
