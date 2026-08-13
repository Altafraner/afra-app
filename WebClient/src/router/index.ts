import { createRouter, createWebHistory } from 'vue-router';
import { routes as otium } from '@/Otium/router/routes';
import { routes as profundum } from '@/Profundum/router/routes';
import { routes as attendance } from '@/Attendance/router/routes';

const routes = [
    {
        path: '/',
        name: 'Home',
        component: () => import('@/views/Dashboard/Home.vue'),
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
        path: '/login',
        name: 'Login',
        component: () => import('@/views/Login.vue'),
        meta: {
            allowAnonymous: true,
        },
    },
    {
        path: '/error/access-denied',
        name: 'Error-Access-Denied',
        component: () => import('@/views/Error/AccessDenied.vue'),
        meta: {
            allowAnonymous: true,
        },
    },
    {
        path: '/error/remote-failure',
        name: 'Error-Remote-Failure',
        component: () => import('@/views/Error/RemoteFailure.vue'),
        meta: {
            allowAnonymous: true,
        },
    },
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('@/views/Error/NotFound.vue'),
        meta: {
            allowAnonymous: true,
        },
    },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;
