<script setup>
import {useUser} from "@/stores/user.js";
import {defineAsyncComponent} from "vue";

const Student = defineAsyncComponent(() => import("@/Otium/views/Dashboard/Student.vue"));
const Teacher = defineAsyncComponent(() => import("@/Otium/views/Dashboard/Teacher.vue"));

const user = useUser()
</script>

<template>
  <Student v-if="user.isStudent"/>
  <Teacher v-if="user.isTeacher"/>
</template>

<style scoped>
p a {
  color: var(--p-primary-500);
}

p a:visited {
  color: var(--p-primary-700);
}
</style>
