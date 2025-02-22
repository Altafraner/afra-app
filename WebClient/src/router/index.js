import { createRouter, createWebHistory } from 'vue-router'
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
    path: "/dashboard",
    name: "Dashboard",
    component: () => import('@/views/DashboardStudent.vue')
  },
  {
    path: "/teacher",
    name: "Teacher",
    component: () => import('@/views/DashboardTeacher.vue')
  },
  {
    path: "/test",
    name: "Test",
    component: () => import('@/views/Test.vue')
  },
  {
    path: "/aufsicht",
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
