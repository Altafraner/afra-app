export const routes = [
    {
        path: '/profundum/katalog',
        name: 'Profundum-Einwahl',
        component: () => import('@/Profundum/views/Einwahl.vue'),
    },
    {
        path: '/profundum/bewertung',
        name: 'ProfundumBewertung',
        component: () => import('@/Profundum/views/Bewertung.vue'),
    }
];
