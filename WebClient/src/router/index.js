import { createRouter, createWebHistory } from 'vue-router'
import MeView from "@/views/MeView.vue";

const routes = [
  {
    path: "/me",
    name: "Meine Übersicht",
    component: MeView
  },
  {
    path: "/katalog",
    name: "Katalog",
    component: () => import('@/views/Katalog/Index.vue')
  },
  {
    path: "/test",
    name: "Test",
    component: () => import('@/views/Test.vue')
  },
  {
    path: "/supervision",
    name: "Aufsicht",
    component:  () => import('@/views/Supervision.vue'),
  },
  {
    path: "/katalog/:date/:block/:otiumId",
    name: "Katalog-Kategorie",
    component: () => import('@/views/Katalog/Termin.vue'),
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
