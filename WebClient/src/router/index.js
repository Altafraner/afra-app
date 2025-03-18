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
    name: "Katalog-datum",
    component: () => import('@/views/Katalog/Index.vue'),
    props: true,
  },
  {
    path: "/katalog/:datum/:block",
    name: "Katalog-datum-block",
    component: () => import('@/views/Katalog/Index.vue'),
    props: true,
  },
  {
    path: "/test",
    name: "Test",
    component: () => import('@/views/Test.vue')
  },
  {
    path: "/aufsicht",
    name: "Aufsicht",
    component: () => import('@/views/Aufsicht.vue'),
  },
  {
    path: "/termin/:terminId",
    name: "Termin",
    component: () => import('@/views/Katalog/Termin.vue'),
    props: true
  },
  {
    path: "/student/:studentId",
    name: "Mentee",
    component: () => import('@/views/Teacher/Mentee.vue'),
    props: true
  },
  {
    path: "/management/termin/:terminId",
    name: "Lehrer-Termin",
    component: () => import('@/views/Teacher/Termin.vue'),
    props: true
  },
  {
    path: '/:pathMatch(.*)*',
    name: "NotFound",
    component: () => import('@/views/NotFound.vue')
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
