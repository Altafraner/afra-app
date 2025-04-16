import {createRouter, createWebHistory} from 'vue-router'
import Home from "@/views/Home.vue";

const routes = [
  {
    path: "/",
    name: "Home",
    component: Home
  },
  {
    path: "/katalog",
    name: "Katalog",
    component: () => import('@/views/Katalog/Index.vue')
  },
  {
    path: "/katalog/:datum",
    name: "Katalog-Datum",
    component: () => import('@/views/Katalog/Index.vue'),
    props: true,
  },
  {
    path: "/termin/:terminId",
    name: "Katalog-Termin",
    component: () => import('@/views/Katalog/Termin.vue'),
    props: true
  },
  {
    path: "/aufsicht",
    name: "Aufsicht",
    component: () => import('@/views/Aufsicht.vue'),
  },
  {
    path: "/student/:studentId",
    name: "Mentee",
    component: () => import('@/views/Teacher/Mentee.vue'),
    props: true
  },
  {
    path: "/management/termin/:terminId",
    name: "Verwaltung-Termin",
    component: () => import('@/views/Teacher/Termin.vue'),
    props: true
  },
  {
    path: '/:pathMatch(.*)*',
    name: "NotFound",
    component: () => import('@/views/NotFound.vue')
  },
  {
    path: '/management',
    name: "Verwaltung",
    component: () => import('@/views/Management/OtiaOverview.vue'),
  },
  {
    path: '/management/otium/:otiumId',
    name: "Verwaltung-Otium",
    component: () => import('@/views/Management/OtiumEdit.vue'),
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
