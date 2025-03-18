<script setup>
import {useUser} from "@/stores/useUser.js";
import Student from "@/views/Dashboard/Student.vue";
import Teacher from "@/views/Dashboard/Teacher.vue";

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
