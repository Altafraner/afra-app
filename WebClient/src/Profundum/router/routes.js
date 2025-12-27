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
    },
    {
        path: '/profundum/management/:profundumId',
        name: 'Profundum-Edit',
        component: () => import('@/Profundum/views/ProfundumEdit.vue'),
        props: true,
    },
];
