import { createRouter, createWebHistory } from 'vue-router';
import Home from '@/views/Home.vue';
import { routes as otium } from '@/Otium/router/routes.js';
import { routes as profundum } from '@/Profundum/router/routes.js';
import LoggedOutHome from '@/views/LoggedOutHome.vue';

const loggedOutRoutes = [
    {
        path: '/',
        name: 'Home',
        component: LoggedOutHome,
    },
    {
        path: '/:pathMatch(?!api/)(.*)*',
        name: 'NotFound',
        component: LoggedOutHome,
    },
];
const loggedInRoutes = [
    {
        path: '/',
        name: 'Home',
        component: Home,
    },
    {
        path: '/admin',
        name: 'Admin',
        component: () => import('@/views/Admin.vue'),
    },
    {
        path: '/settings',
        name: 'Settings',
        component: () => import('@/views/Settings.vue'),
    },
    ...otium,
    ...profundum,
    {
        path: '/:pathMatch(?!api/)(.*)*',
        name: 'NotFound',
        component: () => import('@/views/NotFound.vue'),
    },
];

export function createAppRouter() {
    return createRouter({
        history: createWebHistory(import.meta.env.BASE_URL),
        routes: loggedOutRoutes,
    });
}

function removeRoutes(router, routes) {
    for (const route of routes) {
        if (route.name && router.hasRoute(route.name)) {
            router.removeRoute(route.name);
        }
    }
}

function addRoutes(router, routes) {
    for (const route of routes) {
        router.addRoute(route);
    }
}

export function registerLoggedInRoutes(router) {
    addRoutes(router, loggedInRoutes);
}

export function registerLoggedOutRoutes(router) {
    removeRoutes(router, loggedInRoutes);
    addRoutes(router, loggedOutRoutes);
}
