import { createRouter, createWebHistory } from 'vue-router';
import Home from '@/views/Dashboard/Home.vue';
import { routes as otium } from '@/Otium/router/routes';
import { routes as profundum } from '@/Profundum/router/routes';
import { routes as attendance } from '@/Attendance/router/routes';

const routes = [
    {
        path: '/',
        name: 'Home',
        component: Home,
    },
    {
        path: '/admin/impersonate',
        name: 'Admin-Impersonate',
        component: () => import('@/views/Admin/Impersonate.vue'),
    },
    {
        path: '/admin/cevex',
        name: 'Admin-Cevex',
        component: () => import('@/views/Admin/Cevex.vue'),
    },
    {
        path: '/settings',
        name: 'Settings',
        component: () => import('@/views/Settings.vue'),
    },
    {
        path: '/student/:studentId',
        name: 'Mentee',
        component: () => import('@/views/Dashboard/Mentee.vue'),
        props: true,
    },
    ...otium,
    ...profundum,
    ...attendance,
    {
        path: '/:pathMatch(?!api/)(.*)*',
        name: 'NotFound',
        component: () => import('@/views/NotFound.vue'),
    },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;
