export const routes = [
    {
        path: '/freistellung/neu',
        name: 'Freistellung-Neu',
        component: () => import('@/Freistellung/views/StudentAntrag.vue'),
    },
    {
        path: '/freistellung/meine',
        name: 'Freistellung-Meine',
        component: () => import('@/Freistellung/views/StudentAntragListe.vue'),
    },
    {
        path: '/freistellung/lehrer',
        name: 'Freistellung-Lehrer',
        component: () => import('@/Freistellung/views/LehrerEntscheidung.vue'),
    },
    {
        path: '/freistellung/sekretariat',
        name: 'Freistellung-Sekretariat',
        component: () => import('@/Freistellung/views/SekretariatBearbeitung.vue'),
    },
    {
        path: '/freistellung/schulleiter',
        name: 'Freistellung-Schulleiter',
        component: () => import('@/Freistellung/views/SchulleiterBearbeitung.vue'),
    },
];
