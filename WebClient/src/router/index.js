import {createRouter, createWebHistory} from 'vue-router'
import Home from "@/views/Home.vue";
import {routes as otium} from "@/Otium/router/routes.js";

const routes = [
  {
    path: "/",
    name: "Home",
    component: Home
  },
  ...otium,
  {
    path: '/:pathMatch(.*)*',
    name: "NotFound",
    component: () => import('@/views/NotFound.vue')
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
