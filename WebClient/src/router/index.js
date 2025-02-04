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
    component: () => import('@/views/Katalog.vue')
  },
  {
    path: "/test",
    name: "Test",
    component: () => import('@/views/Test.vue')
  },
  {
    path: "/dynamic/:title/:content",
    name: "Dynamic",
    component:  () => import('@/views/DynamicRoute.vue'),
    props: true,
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
